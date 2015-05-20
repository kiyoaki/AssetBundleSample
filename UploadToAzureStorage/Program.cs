using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace UploadToAzureStorage
{
    class Program
    {
        static readonly ConcurrentBag<string> UploadedUriBag = new ConcurrentBag<string>();

        static void Main(string[] args)
        {
            if (args == null || !args.Any() || args[0] == "help" || args[0] == "man")
            {
                Console.WriteLine("UploadToAzureStorage [TargetDirectory]");
                return;
            }

            var targetDirectory = args[0];
            if (!Directory.Exists(targetDirectory))
            {
                Console.WriteLine("Directory not found. TargetDirectory:{0}", targetDirectory);
                return;
            }

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containerName = CloudConfigurationManager.GetSetting("container");
            var container = blobClient.GetContainerReference(containerName);

            var beforeExistsBlobUris = container.ListBlobs(null, true, BlobListingDetails.All)
                .Select(x => x.Uri.ToString());

            var files = Directory.GetFiles(targetDirectory, "*", SearchOption.AllDirectories);
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 100 }, file =>
            {
                try
                {
                    var assetBundlePath = file.Replace(targetDirectory, "").Replace(@"\", "/");
                    if (assetBundlePath.StartsWith("/"))
                    {
                        assetBundlePath = assetBundlePath.Substring(1);
                    }

                    var blockBlob = container.GetBlockBlobReference(assetBundlePath);
                    using (var fileStream = File.OpenRead(file))
                    {
                        var stopwatch = Stopwatch.StartNew();

                        stopwatch.Start();
                        blockBlob.UploadFromStream(fileStream);
                        stopwatch.Stop();

                        UploadedUriBag.Add(blockBlob.Uri.ToString());

                        var logMessage = GetStats(blockBlob, stopwatch.ElapsedMilliseconds, "Uploaded");
                        Console.WriteLine(logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Upload {0} Error {1}{2}{3}",
                        file, ex.Message, Environment.NewLine, ex.StackTrace);
                }
            });

            Parallel.ForEach(
                beforeExistsBlobUris
                    .Where(x => !UploadedUriBag.Contains(x))
                    .Select(x => container.Uri.MakeRelativeUri(new Uri(x)).ToString().Replace(container.Name + "/", "")),
                new ParallelOptions { MaxDegreeOfParallelism = 100 },
                blobName =>
                {
                    var deleted = container.GetBlockBlobReference(blobName);

                    try
                    {
                        var stopwatch = Stopwatch.StartNew();

                        stopwatch.Start();
                        deleted.Delete();
                        stopwatch.Stop();

                        var logMessage = GetStats(deleted, stopwatch.ElapsedMilliseconds, "Deleted");
                        Console.WriteLine(logMessage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Delete {0} Error {1}{2}{3}",
                            blobName, ex.Message, Environment.NewLine, ex.StackTrace);
                    }
                });
        }

        static string GetReadableByteLength(double byteLength)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            var len = byteLength;
            var order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            return string.Format("{0:0.##}{1}", len, sizes[order]);
        }

        static string GetStats(ICloudBlob blockBlob, long elapsedMilliseconds, string title)
        {
            var byteLength = GetReadableByteLength(blockBlob.Properties.Length);
            var averageSpeed = GetReadableByteLength(
                blockBlob.Properties.Length / ((double)elapsedMilliseconds / 1000)) + "/S";

            return string.Join(Environment.NewLine,
                new[]
                {
                    "",
                    "------------------------",
                    string.Format("{3} {0} Average Speed: {1} Elapsed Time: {2}ms",
                        byteLength, averageSpeed, elapsedMilliseconds, title),
                    string.Format("blob uri: {0}", blockBlob.Uri),
                    string.Format("blobType: {0}", blockBlob.BlobType),
                    string.Format("contentLength: {0}", blockBlob.Properties.Length),
                    string.Format("contentType: {0}", blockBlob.Properties.ContentType),
                    string.Format("contentMD5: {0}", blockBlob.Properties.ContentMD5),
                    "------------------------",
                    ""
                });
        }
    }
}
