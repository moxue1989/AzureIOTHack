using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIoT.Services
{
    public class ImageManager : IImageManager
    {
        private static readonly string StorageName = "mogeneral";
        private static readonly string StorageApiKey = "{STORAGE_KEY}";
        private static readonly string ImageContainer = "azureiot";

        public async Task<string> UploadImageAsync(MemoryStream stream, string imageName)
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                    StorageName, StorageApiKey), true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(ImageContainer);
            CloudBlockBlob blob = cloudBlobContainer.GetBlockBlobReference(imageName.Replace(" ", String.Empty));

            await blob.DeleteIfExistsAsync();
            await blob.UploadFromStreamAsync(stream);

            return blob.Uri.ToString();
        }
    }
}
