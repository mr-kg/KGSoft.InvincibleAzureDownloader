using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace KGSoft.InvincibleAzureDownloader.Services
{
    class StorageAccountProvider
    {
        public static CloudStorageAccount GetStorageAccount(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }
    }
}
