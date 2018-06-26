

namespace MoveFilesToAzureBlob
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.DataMovement;
    class Program
    {
        static void Main(string[] args)
        {
            //Test 
            //string storageConnectionStr = "";
            //MoveFiles(@"\\MININT-BP3524V\Files", storageConnectionStr);
            Console.WriteLine("Please enter the share folder (without file name): ");
            string folderPath = Console.ReadLine();

            Console.WriteLine("Please enter the storage account connection string:");
            string stroageConnectionString = Console.ReadLine();

            MoveFiles(folderPath, stroageConnectionString);
        }

        static void MoveFiles(string _folderPath, string _storageConnectionString, 
            string _containerName = "files" )
        {
            string storageConnectionString = _storageConnectionString;
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_containerName);
            blobContainer.CreateIfNotExistsAsync();

            //Go through all the files and upload one by one
            var files = Directory.GetFiles(_folderPath);
            foreach (var fileToUpload in files)
            {
                int pos = fileToUpload.LastIndexOf("\\") + 1;
                string _fileName = fileToUpload.Substring(pos, fileToUpload.Length - pos);
                string sourcePath = fileToUpload;
                CloudBlockBlob destBlob = blobContainer.GetBlockBlobReference(_fileName);

                // Setup the number of the concurrent operations
                TransferManager.Configurations.ParallelOperations = 64;
                // Setup the transfer context and track the upoload progress
                SingleTransferContext context = new SingleTransferContext();
                context.ProgressHandler = new Progress<TransferStatus>((progress) =>
                {
                    Console.WriteLine("Bytes uploaded: {0}", progress.BytesTransferred);
                });
                try
                {
                    // Upload a local blob
                    var task = TransferManager.UploadAsync(
                        sourcePath, destBlob, null, context, CancellationToken.None);
                    task.Wait();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
            }
            
        }
    }
}
