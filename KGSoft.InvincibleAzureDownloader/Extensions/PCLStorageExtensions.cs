using PCLStorage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KGSoft.InvincibleAzureDownloader.Extensions
{
    static class PCLStorageExtensions
    {
        public static async void CopyFileTo(this IFile file, IFolder destinationFolder, CancellationToken cancellationToken = default(CancellationToken))
        {
            var destinationFile =
                await destinationFolder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting, cancellationToken);

            using (var outFileStream = await destinationFile.OpenAsync(FileAccess.ReadAndWrite, cancellationToken))
            using (var sourceStream = await file.OpenAsync(FileAccess.Read, cancellationToken))
            {
                await sourceStream.CopyToAsync(outFileStream, 81920, cancellationToken);
            }
        }

        public static async Task<long> GetFileLength(this IFile file)
        {
            using (var s = await file.OpenAsync(FileAccess.Read))
            {
                return s.Length;
            }
        }

        public static async Task<IFile> CreateFileIfNotExists(this IFolder folder, string filename)
        {
            if (await folder.CheckExistsAsync(filename) == ExistenceCheckResult.FileExists)
                return await folder.GetFileAsync(filename);
            else
                return await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<bool> DeleteFileIfExists(this IFolder folder, string filename)
        {
            if (await folder.CheckExistsAsync(filename) == ExistenceCheckResult.FileExists)
            {
                try
                {
                    var file = await folder.GetFileAsync(filename);
                    await file.DeleteAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}
