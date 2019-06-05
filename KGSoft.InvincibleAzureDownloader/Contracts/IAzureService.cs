using KGSoft.InvincibleAzureDownloader.Model;
using PCLStorage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KGSoft.InvincibleAzureDownloader.Contracts
{
    public interface IAzureService
    {
        void Initialize(string connectionString);
        void SetContainer(string containerName);
        Task DownloadAsync(string localFolderPath, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null);
        Task DownloadAsync(IFolder localFolder, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default, EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged = null);
    }
}
