using KGSoft.InvincibleAzureDownloader.Contracts;
using KGSoft.InvincibleAzureDownloader.Model;
using Microsoft.WindowsAzure.Storage.Blob;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KGSoft.InvincibleAzureDownloader.Services
{
    public class AzureBlobStorageService : IAzureService
    {
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        private FileHandler _fileHandler;
        private int _downloadBufferSize;

        /// <summary>
        /// Initialize a service to download from Azure Blob Storage
        /// </summary>
        /// <param name="connectionString">The Azure Storage Connection String</param>
        /// <param name="defaultContainer">The default blob container to set</param>
        /// <param name="downloadBufferSizeBytes">The buffer/download chunk size</param>
        /// <param name="localDirectorySplitChar">The character used to denote local filesystem directories</param>
        public AzureBlobStorageService(string connectionString, string defaultContainer = null, int downloadBufferSizeBytes = 2097152, char localDirectorySplitChar = '\\')
        {
            Initialize(connectionString);
            if (!string.IsNullOrEmpty(defaultContainer))
                SetContainer(defaultContainer);
            _downloadBufferSize = downloadBufferSizeBytes;
            _fileHandler = new FileHandler(_downloadBufferSize, localDirectorySplitChar);
        }

        public async Task DownloadAsync(string localFolderPath, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null)
        {
            var blobReference = await GetBlob(cloudResourceReference);
            var folder = await FileSystem.Current.GetFolderFromPathAsync(localFolderPath);
            await _fileHandler.WriteFile(localFilename, blobReference, folder, writeToRoot, token, OnDownloadProgressChanged);
        }

        public async Task DownloadAsync(IFolder localFolder, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null)
        {
            var blobReference = await GetBlob(cloudResourceReference);
            await _fileHandler.WriteFile(localFilename, blobReference, localFolder, writeToRoot, token, OnDownloadProgressChanged);
        }

        private async Task<CloudBlockBlob> GetBlob(string cloudResourceReference)
        {
            var blobReference = _blobContainer.GetBlockBlobReference(cloudResourceReference);
            await blobReference.FetchAttributesAsync();
            return blobReference;
        }

        public void Initialize(string connectionString)
        {
            _blobClient = StorageAccountProvider
                .GetStorageAccount(connectionString)
                .CreateCloudBlobClient();
        }

        public void SetContainer(string containerName)
        {
            if (_blobContainer == null || _blobContainer.Name != containerName)
            {
                _blobContainer = _blobClient.GetContainerReference(containerName);
                _blobContainer.CreateIfNotExistsAsync().Wait();
            }
        }
    }
}
