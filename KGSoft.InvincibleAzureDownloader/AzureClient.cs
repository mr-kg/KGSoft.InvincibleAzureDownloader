using KGSoft.InvincibleAzureDownloader.Contracts;
using KGSoft.InvincibleAzureDownloader.Model;
using PCLStorage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KGSoft.InvincibleAzureDownloader
{
    public class AzureClient
    {
        private readonly IAzureService _azureService;

        /// <summary>
        /// A progress callback so we can monitor and report download progress
        /// </summary>
        public event EventHandler<FileDownloadProgressCallback> OnDownloadProgressChanged;

        /// <summary>
        /// Initialize the Azure client here.
        /// </summary>
        /// <param name="azureService">Inject a service of your choice. Ie. AzureBlobStorageService or AzureFileStorageService</param>
        public AzureClient(IAzureService azureService)
        {
            _azureService = azureService;
        }

        /// <summary>
        /// Asynchronously download an Azure resource to a file path
        /// </summary>
        /// <param name="localFolderPath">The path to the local folder you wish to download to</param>
        /// <param name="localFilename">The desired destination filename of the resource you will download</param>
        /// <param name="cloudResourceReference">The cloud resource reference name (ie. filename or blob name)</param>
        /// <param name="writeToRoot">Write to root of the local folder, or create a nested file structure to mirror what is in the Azure Storage</param>
        /// <param name="token">A cancellation Token =</param>
        /// <returns></returns>
        public Task DownloadAsync(string localFolderPath, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default)
        {
            return _azureService.DownloadAsync(localFolderPath, localFilename, cloudResourceReference, writeToRoot, token, OnDownloadProgressChanged);
        }

        /// <summary>
        /// Asynchronously download an Azure resource to a PCLStorage IFolder
        /// </summary>
        /// <param name="folder">PCLStorage IFolder to download to</param>
        /// <param name="localFilename">The desired destination filename of the resource you will download</param>
        /// <param name="cloudResourceReference">The cloud resource reference name (ie. filename or blob name)</param>
        /// <param name="writeToRoot">Write to root of the local folder, or create a nested file structure to mirror what is in the Azure Storage</param>
        /// <param name="token">A cancellation Token =</param>
        /// <returns></returns>
        public Task DownloadAsync(IFolder folder, string localFilename, string cloudResourceReference, bool writeToRoot = false, CancellationToken token = default)
        {
            return _azureService.DownloadAsync(folder, localFilename, cloudResourceReference, writeToRoot, token, OnDownloadProgressChanged);
        }
    }
}
