using System;

namespace KGSoft.InvincibleAzureDownloader.Test
{
    class Program
    {
        static string _connectionString = "";
        static string _initialBlobContainer = "";
        static string _initialFileShare = "";

        static void Main(string[] args)
        {
            TestBlobDownload("", "", "");
            TestFileshareDownload("", "", "");
        }

        private static void Client_OnDownloadProgressChanged(object sender, Model.FileDownloadProgressCallback e)
        {
            Console.WriteLine($"{e.CompletionPercentage}%");
        }

        private static void TestFileshareDownload(string localFolder, string localFilename, string resourceName)
        { 
            var service = new Services.AzureFileStorageService(_connectionString, _initialFileShare);
            var client = new AzureClient(service);

            client.OnDownloadProgressChanged += Client_OnDownloadProgressChanged;
            client.DownloadAsync(localFolder, localFilename, resourceName).Wait();
            client.OnDownloadProgressChanged -= Client_OnDownloadProgressChanged;

            Console.WriteLine("Done!");
        }

        private static void TestBlobDownload(string localFolder, string localFilename, string resourceName)
        {
            var service = new Services.AzureBlobStorageService(_connectionString, _initialBlobContainer);
            var client = new AzureClient(service);

            var folder = PCLStorage.FileSystem.Current.GetFolderFromPathAsync(localFolder).Result;

            client.OnDownloadProgressChanged += Client_OnDownloadProgressChanged;
            client.DownloadAsync(folder, localFilename, resourceName, false).Wait();
            client.OnDownloadProgressChanged -= Client_OnDownloadProgressChanged;

            Console.WriteLine("Done!");
        }
    }
}
