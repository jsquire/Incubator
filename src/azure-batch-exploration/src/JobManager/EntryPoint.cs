using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Batch.FileStaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace JobManager
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            var settingEnvironmentKey = args[0] ?? throw new ArgumentException("The job settings environment key is expected as the first argument", nameof(args));
            var jobId                 = args[1] ?? throw new ArgumentException("job identifier is expected as the second argument", nameof(args));
            var containerName         = args[2] ?? throw new ArgumentException("blob container is expected as the third argument", nameof(args));
            var blobName              = args[3] ?? throw new ArgumentException("blob name is expected as the fourth argument", nameof(args));
            var serializedSettings    = Environment.GetEnvironmentVariable(settingEnvironmentKey) ?? throw new ArgumentException("The job settings key did not refer to a valid environment variable", nameof(args));
            
            EntryPoint.ProcessAsync(JsonConvert.DeserializeObject<JobSettings>(serializedSettings), jobId, containerName, blobName).GetAwaiter().GetResult();
        }

        private static async Task ProcessAsync(JobSettings settings, string jobId, string containerName, string blobName)
        {
            // Simulate splitting of the target blob into chunks by just making some copies of the source blob.

            var client        = CloudStorageAccount.Parse(settings.BatchBlobStorageConnection).CreateCloudBlobClient();
            var container     = client.GetContainerReference(containerName);
            var blob          = (CloudBlockBlob)container.GetBlobReferenceFromServer(blobName);
            var leaseDuration = TimeSpan.FromSeconds(60);
            var leaseName     = await blob.AcquireLeaseAsync(leaseDuration).ConfigureAwait(false);
            var lease         = AccessCondition.GenerateLeaseCondition(leaseName);
            var copies        = await Task.WhenAll(Enumerable.Range(0, 5).Select(index => EntryPoint.CopyBlobAsync(container, blob, $"{ Path.GetFileNameWithoutExtension(blobName) }-{ index }{ Path.GetExtension(blobName) }", lease, leaseDuration))).ConfigureAwait(false);
                        
            blob.Delete(DeleteSnapshotsOption.IncludeSnapshots, lease);


            // Create tasks for the job that will process each chunk.  In this case, that will be one of the copies that we
            // made to simulate it.

            using (var batchClient = await BatchClient.OpenAsync(new BatchSharedKeyCredentials(settings.BatchAccountUrl, settings.BatchAccountName, settings.BatchAccountKey)))
            {
                // Add a retry policy. The built-in policies are No Retry (default), Linear Retry, and Exponential Retry.

                batchClient.CustomBehaviors.Add(RetryPolicyProvider.ExponentialRetryProvider(TimeSpan.FromSeconds(settings.RetryDeltaBackoff), settings.RetryMaxCount));

                try
                {
                    // Create tasks with file references here.  I'm reusing the existing resource list, since they've already been staged. 
                    // For input, in our case, we're letting the task access the blob directly.   You could also use file staging here, if you'd rather the task not have
                    // knowledge that it is working on a blob directly.  However, since you most likely want to process it for redaction and then upload the result,
                    // it seems like a better idea to work through storage for both.
                                        
                    var moverPath    = $"{ typeof(BlobMover.EntryPoint).Assembly.GetName().Name }.exe";
                    var jobResources = batchClient.JobOperations.GetJob(jobId)?.JobManagerTask.ResourceFiles;

                    jobResources.Select(thing => { Console.WriteLine(thing); return thing; });

                    var copyTasks = copies
                        .Where(result => result.Created)
                        .Select(result =>  new CloudTask(Path.GetFileNameWithoutExtension(result.Name), $"{ moverPath } { containerName } { result.Name } {settings.BatchBlobStorageConnection }")
                        {
                            ResourceFiles = jobResources
                        });

                    await batchClient.JobOperations.AddTaskAsync(jobId, copyTasks).ConfigureAwait(false);

                    // Wait for all of the work associated with processing the chunks to complete.

                    await EntryPoint.WaitForChildTasksAsync(batchClient, jobId, TimeSpan.FromHours(2)).ConfigureAwait(false);

                    // This is where we would create the final tasks to process the results for each of the 
                    // processed chunks.  After, we would have to perform another WaitForTasksAsync to ensure
                    // that it is complete before cleaning up.
                    //
                    // Because we just simulated work by copying the fake chunk blobs, I skipped those steps.  The code
                    // would be virtually identical to the other code in the try block above.   
                    //
                    // Alternatively, you could perform the final processing directly here. 
                }

                catch (Exception ex)
                {
                    // Surfacing information from failed tasks can be a challenge.  I suspct that there are more efficient and easier ways to do so, but
                    // for my purposes, a rudimentary capture and upload to blob store was very helpful.
                    //
                    // NOTE:  This catch block is doing a bunch of things, and things that could fail.  It goes without saying, this isn't best
                    //        practice.

                    var outblob = container.GetBlockBlobReference("task-errors.txt");
                    
                    using (var memStream = new MemoryStream())
                    using (var writer = new StreamWriter(memStream))
                    {
                        writer.WriteLine(ex.GetType().Name);
                        writer.WriteLine(ex.Message);
                        writer.WriteLine();
                        writer.WriteLine(ex.StackTrace);
                        writer.WriteLine();

                        if (ex.InnerException != null)
                        {
                            writer.WriteLine(ex.InnerException.GetType().Name);
                            writer.WriteLine(ex.InnerException.Message);
                            writer.WriteLine();
                            writer.WriteLine(ex.InnerException.StackTrace);
                            writer.WriteLine();
                        }
                                                
                        writer.Flush();
                        memStream.Position = 0;
                        outblob.UploadFromStream(memStream);

                        writer.Close();
                        memStream.Close();
                    }


                    await batchClient.JobOperations.TerminateJobAsync(jobId).ConfigureAwait(false);
                    throw;
                }

                finally
                {    
                    // Clean the resource container used for job file storage.  
                    //
                    // If we used file staging rather than blob storage access to seed the individual processing jobs, we'd have to clean those 
                    // here as well.  Those are a bit awkward to get;  you have to create a bag to hold and discover them when the task was added
                    // to the job.   See line 205 of https://github.com/Azure/azure-batch-samples/blob/master/CSharp/GettingStarted/02_PoolsAndResourceFiles/JobSubmitter/JobSubmitter.cs
                                        
                    await client.GetContainerReference(settings.JobResourceContainerName).DeleteIfExistsAsync().ConfigureAwait(false);

                    // Delete the job to ensure the tasks are cleaned up

                    if  ((!String.IsNullOrEmpty(jobId)) && (settings.ShouldDeleteJob))
                    {                        
                        await batchClient.JobOperations.DeleteJobAsync(jobId).ConfigureAwait(false);
                    }
                }
            }
        }

        private static async Task<IEnumerable<string>> SubmitMoveTasksAsync(JobSettings settings, BatchClient batchClient, string blobContainerName, IEnumerable<string> blobNames, string jobId)
        {
            if (String.IsNullOrEmpty(jobId))
            {
                throw new ArgumentNullException(nameof(jobId));
            }
                        
            // Create the mover task, ensuring that the needed executable is staged

            var moverExe              = $"{ typeof(BlobMover.EntryPoint).Assembly.GetName().Name }.exe";
            var fileArtifacts         = new ConcurrentBag<ConcurrentDictionary<Type, IFileStagingArtifact>>();
            var stagingStorageAccount = new StagingStorageAccount(settings.BatchBlobStorageName, settings.BatchBlobStorageKey, settings.BatchBlobSTorageUrl);
            var moverFilesToStage     = new List<IFileStagingProvider> { new FileToStage($"{ moverExe }", stagingStorageAccount) };            
            var moverCloudTasks       = blobNames.Select(blobName => new CloudTask($"Mover-{ blobName }", $"{ moverExe } { blobContainerName } { blobName }") { FilesToStage = moverFilesToStage });

            await batchClient.JobOperations.AddTaskAsync(jobId, moverCloudTasks, fileStagingArtifacts: fileArtifacts).ConfigureAwait(false);

            return fileArtifacts
                .SelectMany(dict => dict).Select(kvp => kvp.Value)
                .OfType<SequentialFileStagingArtifact>()
                .Select(artifact => artifact.BlobContainerCreated)
                .Distinct();
        }

        private static async Task WaitForChildTasksAsync(BatchClient batchClient, string jobId, TimeSpan timeout)
        {
            // To figure out if the job is complete, wait for all tasks that aren't the job manager task (which is the context that this code runs in.)
            // If you con't filter out the job manager, then the job will never complete, as the job manager task will block waiting for completion of
            // the job manager task.

            var job              = await batchClient.JobOperations.GetJobAsync(jobId).ConfigureAwait(false);
            var managerTaskId    = job.JobManagerTask.Id;
            var childTasks       = job.ListTasks().Where(task => String.Compare(task.Id,managerTaskId, StringComparison.OrdinalIgnoreCase) != 0);
            var taskStateMonitor = batchClient.Utilities.CreateTaskStateMonitor();

            await taskStateMonitor.WhenAll(childTasks, TaskState.Completed, timeout).ConfigureAwait(false);
        }
                
        private static async Task<CopyResult> CopyBlobAsync(CloudBlobContainer container, CloudBlockBlob blob, string newBlobName, AccessCondition lease, TimeSpan leaseDuration)
        {             
            var newBlob           = container.GetBlockBlobReference(newBlobName);
            var delayMilliseconds = TimeSpan.FromMilliseconds(500);
            var maxDelay          = leaseDuration.Subtract(leaseDuration.Subtract(TimeSpan.FromSeconds(5)));
            var startTime         = DateTime.UtcNow;
            
            await newBlob.StartCopyAsync(blob).ConfigureAwait(false);

            while (newBlob.CopyState.Status == CopyStatus.Pending)
            {
                await Task.Delay(delayMilliseconds).ConfigureAwait(false);

                // The lease can be held for a finite period.  If we get close to that, ask for
                // renewal.

                if (DateTime.UtcNow.Subtract(startTime) >= maxDelay)
                {                
                    await blob.RenewLeaseAsync(lease).ConfigureAwait(false);
                    startTime = DateTime.UtcNow;
                }
            }

            return new CopyResult(newBlobName, newBlob.CopyState.Status != CopyStatus.Failed);        
        }

        private class CopyResult
        {
            public string Name;
            public bool Created;
            public CopyResult(string name, bool created) { this.Name = name; this.Created = created; }
        }
    }
}
