using Azure.Storage.Blobs;
using ABC_Inc_Project_CLD7112.Services;
using Microsoft.Extensions.Configuration;

namespace ABC_Inc_Project_CLD7112.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient("product-images");
            _containerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = contentType
            });
            return blobClient.Uri.ToString();
        }
    }
}