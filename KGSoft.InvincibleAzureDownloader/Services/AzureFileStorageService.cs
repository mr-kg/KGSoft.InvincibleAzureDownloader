using KGSoft.InvincibleAzureDownloader.Contracts;
using KGSoft.InvincibleAzureDownloader.Model;
using Microsoft.WindowsAzure.Storage.File;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KGSoft.InvincibleAzureDownloader.Services
{
    public class AzureFileStorageService : IAzureService
    {
        private CloudFileClient _fileClient;
        private CloudFileShare _fileShare;
        private FileHandler _fileHandler;
        private int _downloadBufferSize;

        public AzureFileStorageService(string connectionString, string defaultContainer = null, int downloadBufferSizeBytes = 2097152, char localDirectorySplitChar = '\\')
        {
            Initialize(connectionString);
            if (!string.IsNullOrEmpty(defaultContainer))
                SetContainer(defaultContainer);
            _downloadBufferSize = downloadBufferSizeBytes;
            _fileHandler = new FileHandler(_downloadBufferSize, localDirectorySplitChar);
        }

        public async Task DownloadAsync(string localFolderPath, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null)
        {
            var file = GetFile(cloudResourceReference);
            var folder = await FileSystem.Current.GetFolderFromPathAsync(localFolderPath);
            await _fileHandler.WriteFile(localFilename, file, folder, writeToRoot, token, OnDownloadProgressChanged);
        }

        public async Task DownloadAsync(IFolder localFolder, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null)
        {
            var file = GetFile(cloudResourceReference);
            await _fileHandler.WriteFile(localFilename, file, localFolder, writeToRoot, token, OnDownloadProgressChanged);
        }

        public void Initialize(string connectionString)
        {
            _fileClient = StorageAccountProvider
                .GetStorageAccount(connectionString)
                .CreateCloudFileClient();
        }

        public void SetContainer(string containerName)
        {
            if (_fileShare == null || _fileShare.Name != containerName)
            {
                _fileShare = _fileClient.GetShareReference(containerName);
                _fileShare.CreateIfNotExistsAsync().Wait();
            }
        }

        private async Task<CloudFile> GetFile(string filename)
        {
            var root = _fileShare.GetRootDirectoryReference();
            var segments = filename.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == _fileShare.Name)
                segments = segments.Skip(1).ToArray();

            if (segments.Count() > 1)
                for (int i = 0; i < segments.Length - 1; i++)
                {
                    try
                    {
                        root = root.GetDirectoryReference(segments[i]);
                    }
                    catch
                    {
                        break;
                    }
                }

            var file = root.GetFileReference(segments.Last());
            await file.FetchAttributesAsync();
            return file;
        }
    }
}
