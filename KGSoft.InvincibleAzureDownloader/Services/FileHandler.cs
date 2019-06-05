using KGSoft.InvincibleAzureDownloader.Extensions;
using KGSoft.InvincibleAzureDownloader.Model;
using Microsoft.WindowsAzure.Storage.Blob;
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
    class FileHandler
    {
        char _localPathSplitChar;
        int _bufferSize;
        long _cloudResourceLength;
        CloudBlockBlob _blob;
        CloudFile _file;

        public FileHandler(int bufferSize, char localPathSplitChar = '\\')
        {
            _bufferSize = bufferSize;
            _localPathSplitChar = localPathSplitChar;
        }

        public async Task WriteFile<T>(string filename, T resource, IFolder folder, bool writeToRoot, CancellationToken token, EventHandler<FileDownloadProgressCallback> handler)
        {
            var segments = filename.Split(new char[] { _localPathSplitChar }, StringSplitOptions.RemoveEmptyEntries).ToList();
            IFile destinationFile = null;

            HandleResourceType(resource);

            if (writeToRoot)
            {
                // Jump straight to the file and don't create the while dir tree
                // Pick up the temp file if it exists
                destinationFile = await folder.CreateFileIfNotExists(segments.Last() + ".temp");
                await StreamToFile(destinationFile, token, handler);
            }
            else
            {
                // Walk the dir tree, creating folders as we go
                foreach (var segment in segments)
                {
                    // Check if we are still in a directory
                    if (segment != segments.Last())
                    {
                        if (await folder.CheckExistsAsync(segment) != ExistenceCheckResult.FolderExists)
                            folder = await folder.CreateFolderAsync(segment, CreationCollisionOption.OpenIfExists);
                        else
                            folder = await folder.GetFolderAsync(segment);
                    }
                    // We have arrived at the file
                    else
                    {
                        destinationFile = await folder.CreateFileIfNotExists(segment + ".temp");
                        await StreamToFile(destinationFile, token, handler);
                    }
                }
            }

            // Finally, rename the temp file
            var originalFilename = destinationFile.Name.Replace(".temp", string.Empty);
            if (await folder.DeleteFileIfExists(originalFilename))
                await destinationFile.RenameAsync(originalFilename);
        }

        private async Task StreamToFile(IFile file, CancellationToken token, EventHandler<FileDownloadProgressCallback> handler)
        {
            var length = await file.GetFileLength();

            while (length < _cloudResourceLength)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    if (_blob != null)
                        await _blob.DownloadRangeToStreamAsync(ms, length, _bufferSize, null, null, null, token);
                    else if (_file != null)
                        await _file.DownloadRangeToStreamAsync(ms, length, _bufferSize, null, null, null, token);
                    else
                        throw new Exception("Resource is not assigned!");

                    using (var outFileStream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
                    {
                        var content = ms.ToArray();
                        outFileStream.Position = length;
                        outFileStream.Write(content, 0, content.Length);
                        length += content.Length;
                        handler?.Invoke(this, new FileDownloadProgressCallback(length, _cloudResourceLength));
                    }
                }
            }
        }

        private void HandleResourceType(object resource)
        {
            if (resource.GetType() == typeof(CloudBlockBlob))
            {
                _blob = (CloudBlockBlob)(object)resource;
                _cloudResourceLength = _blob.Properties.Length;
            }
            else if (resource.GetType() == typeof(CloudFile))
            {
                _file = (CloudFile)(object)resource;
                _cloudResourceLength = _file.Properties.Length;
            }
            else
                throw new Exception("Invalid resource type. Resource must be a downloadable Azure resource (ie. CloudBlockBlob");
        }
    }
}
