using System;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobMover
{
    public class EntryPoint
    {
        public static int Main(string[] args)
        {
            var containerName     = args[0] ?? throw new ArgumentException("blob container is expected as the first argument");
            var blobName          = args[1] ?? throw new ArgumentException("blob name is expected as the second argument");
            var connection        = args[2] ?? throw new ArgumentException("blob storage connection string is expected as the third argument");
            var client            = CloudStorageAccount.Parse(connection).CreateCloudBlobClient();
            var container         = client.GetContainerReference(containerName);
            var blob              = container.GetBlockBlobReference(blobName);
            var leaseDuration     = TimeSpan.FromSeconds(60);
            var leaseName         = blob.AcquireLease(leaseDuration, String.Empty);
            var lease             = AccessCondition.GenerateLeaseCondition(leaseName);
            var delayMilliseconds = TimeSpan.FromMilliseconds(500);
            var maxDelay          = leaseDuration.Subtract(leaseDuration.Subtract(TimeSpan.FromSeconds(5)));
            var startTime         = DateTime.UtcNow;
            var newBlob           = container.GetBlockBlobReference($"new-{ blobName }");

            try
            {            
                newBlob.StartCopy(blob);

                while (newBlob.CopyState.Status == CopyStatus.Pending)
                {
                    Thread.Sleep(delayMilliseconds);

                    // The lease can be held for a finite period.  If we get close to that, ask for
                    // renewal.

                    if (DateTime.UtcNow.Subtract(startTime) >= maxDelay)
                    {                
                        blob.RenewLease(lease);
                        startTime = DateTime.UtcNow;
                    }
                }


                if (newBlob.CopyState.Status == CopyStatus.Failed)
                {
                   throw new StorageException($"Blob copy failed for { newBlob.Name }");
                }

                blob.Delete(DeleteSnapshotsOption.IncludeSnapshots, lease);
            }

            catch (Exception ex)
            {
                // Surfacing information from failed tasks can be a challenge.  I suspct that there are more efficient and easier ways to do so, but
                // for my purposes, a rudimentary capture and upload to blob store was very helpful.
                //
                // NOTE:  This catch block is doing a bunch of things, and things that could fail.  It goes without saying, this isn't best
                //        practice.

                var outblob = container.GetBlockBlobReference($"{ Guid.NewGuid().ToString("N") }-errors.txt");
                    
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

                return -1;
            }

            return 0;
        }
    }
}
