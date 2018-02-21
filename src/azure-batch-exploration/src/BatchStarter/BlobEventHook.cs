using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JobManager;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace BatchStarter
{
    public static class BlobEventHook
    {
        [FunctionName(nameof(BlobEventHook))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Batch Job Requested...");

            try
            {
                var settings = JobSettings.FromAppSettings(); 

                using (var batchClient = await BatchClient.OpenAsync(new BatchSharedKeyCredentials(settings.BatchAccountUrl, settings.BatchAccountName, settings.BatchAccountKey)))
                {
                    // Add a retry policy. The built-in policies are No Retry (default), Linear Retry, and Exponential Retry.

                    batchClient.CustomBehaviors.Add(RetryPolicyProvider.ExponentialRetryProvider(TimeSpan.FromSeconds(settings.RetryDeltaBackoff), settings.RetryMaxCount));

                    var jobId     = $"BlobMover-{ Guid.NewGuid().ToString() }";
                    var queryArgs = req.GetQueryNameValuePairs();
                    var container = queryArgs.FirstOrDefault(kvp => String.Equals(kvp.Key, "container", StringComparison.InvariantCultureIgnoreCase)).Value;
                    var blob      = queryArgs.FirstOrDefault(kvp => String.Equals(kvp.Key, "blob", StringComparison.InvariantCultureIgnoreCase)).Value;                    
                
                    await BlobEventHook.SubmitJobAsync(settings, batchClient, container, blob, jobId, log);

                    log.Info("Batch Job Created.");

                    return req.CreateResponse(HttpStatusCode.OK, nameof(HttpStatusCode.OK));
                } 
            }

            catch (Exception ex)
            {
                log.Info("");
                log.Error("An error occurred while submitting the job", ex);
                log.Info("");

                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }            
        }

        private static async Task SubmitJobAsync(JobSettings settings, BatchClient batchClient, string blobContainerName, string blobName, string jobId, TraceWriter log)
        {
            if (String.IsNullOrEmpty(blobName))
            {
                throw new ArgumentNullException(nameof(blobName));
            }

            if (String.IsNullOrEmpty(blobContainerName))
            {
                throw new ArgumentNullException(nameof(blobContainerName));
            }

            if (String.IsNullOrEmpty(jobId))
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            // Prepare the resoures needed to run the job.  This involves staging them into a blob storage container and bulding metadata
            // to describe to the initial job manager task what resources it depends on and where to locate them for staging to the virtual
            // machines which will execute the tasks.
                        
            var blobClient        = CloudStorageAccount.Parse(settings.BatchBlobStorageConnection).CreateCloudBlobClient();
            var resourceContainer = blobClient.GetContainerReference(settings.JobResourceContainerName);
            var retryPolicy       = new Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry(TimeSpan.FromSeconds(settings.RetryDeltaBackoff), settings.RetryMaxCount);

            await resourceContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions { RetryPolicy = retryPolicy }, null).ConfigureAwait(false);

            var resourceFilePaths = Directory.EnumerateFiles("..\\..\\Resources").Where(file => ".dll|.exe|.config".Contains(Path.GetExtension(file)));
            var resourceFiles     = await BlobEventHook.PrepareResources(resourceContainer, resourceFilePaths).ConfigureAwait(false);
                       
            // For this job, ask the Batch service to automatically create a pool of VMs when the job is submitted.
            // If we were doing the parallel processing, we would probably want to specify a bigger target set of compute nodes
            // and potentially set the task policy.  
            //
            // See: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.batch.poolspecification?view=azure-dotnet
            //
            // We may also consider allowing a longer-lived or semi-permanent pool if we're seeing a high load of files to be processed.  My goal
            // was go try and keep costs controlled by using a tiny machine on-demand that goes away quickly.  Billing for Batch is equivilent to runtime
            // for the number and size of VMs used.
            //
            // The OS samily values for cloud services is an opaque string that is expected to be a family series number.  For
            // the OS family values, see: https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-guestos-update-matrix


            var poolInformation = BlobEventHook.CreateJobPoolInformation
            (
                prefix        : "BlobMover", 
                nodeCount     : settings.PoolTargetNodeCount,
                cloudOsFamily : settings.PoolOsFamily,
                vmSize        : settings.PoolNodeVirtualMachineSize
            );
                        
            // Create the job and associate it with a dedicated management task; The management task will ensure that the work is coordinated and
            // the job cleanup is performed so that the function doesn't have to own responsibility for it.
            //
            // The job manager is receiving some arguments from it's command line, while the JSON blob for its settings is being passed via an
            // environment variable.  This environment variable is available just like any other on the machine where the task is run.  It can be
            // accessed without forcing a dependency on Azure Batch into the target.

            var jobSettingsKey        = "JobSettings";
            var jobSettings           = JsonConvert.SerializeObject(settings);
            var job                   = batchClient.JobOperations.CreateJob(jobId, poolInformation);

            var managerPath           = $"{ typeof(JobManager.EntryPoint).Assembly.GetName().Name }.exe";
            var managerCommand        = $"{ managerPath } { jobSettingsKey } { jobId } { blobContainerName } { blobName }";
            
                        
            log.Info($"Configuring job manager with the command line: \"{ managerCommand }\'");
            
            job.JobManagerTask = new JobManagerTask("JobManager", managerCommand)
            {
                KillJobOnCompletion = true,
                ResourceFiles       = resourceFiles,
                EnvironmentSettings = new List<EnvironmentSetting> { new EnvironmentSetting(jobSettingsKey, jobSettings) }
            };
                                    
            // The job isn't "real" to the Azure Batch service until it's committed.
              
            await job.CommitAsync().ConfigureAwait(false);
        }
                
        private static Task<ResourceFile[]> PrepareResources(CloudBlobContainer container, IEnumerable<string> filePaths)
        {
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(2)
            };

            return Task.WhenAll(filePaths.Select(filePath => BlobEventHook.CreateBlobResource(container, filePath, sasPolicy)));
        }

        private static async Task<ResourceFile> CreateBlobResource(CloudBlobContainer container, string filePath, SharedAccessBlobPolicy sasPolicy)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {       
                var fileName = Path.GetFileName(filePath);
                var blob     = container.GetBlockBlobReference(fileName);

                await blob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
                fileStream.Close();


                return new ResourceFile($"{ blob.Uri }{ blob.GetSharedAccessSignature(sasPolicy) }", fileName);
            }
        }

        private static PoolInformation CreateJobPoolInformation(string prefix, int nodeCount, string cloudOsFamily, string vmSize) =>
            new PoolInformation
            {
                AutoPoolSpecification = new AutoPoolSpecification
                {
                    AutoPoolIdPrefix   = prefix,
                    KeepAlive          = false,
                    PoolLifetimeOption = PoolLifetimeOption.Job,

                    PoolSpecification = new PoolSpecification
                    {
                        //TargetLowPriorityComputeNodes = nodeCount, // This is the cheaper option if the tasks aren't time sensitive.                       
                        TargetDedicatedComputeNodes   = nodeCount, // This is the more expensive option, but stil has an element of non-determinism in here for task scheduling.
                        CloudServiceConfiguration     = new CloudServiceConfiguration(cloudOsFamily),
                        VirtualMachineSize            = vmSize                        
                        
                    }
                }
            };        
    }
}
