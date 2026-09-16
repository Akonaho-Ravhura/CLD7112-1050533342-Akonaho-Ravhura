using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;

namespace ABC_Inc_Project_CLD7112.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ShareClient _shareClient;
        private const string LogFileName = "application-log.txt";
        private const int MaxFileSize = 1024 * 1024; // 1 MB

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _shareClient = new ShareClient(connectionString, "logs");
            _shareClient.CreateIfNotExists();
        }

        private async Task<ShareFileClient> GetLogFileClientAsync()
        {
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(LogFileName);

            if (!await fileClient.ExistsAsync())
            {
                await fileClient.CreateAsync(MaxFileSize);
            }

            return fileClient;
        }

        public async Task WriteLogAsync(string logMessage)
        {
            var fileClient = await GetLogFileClientAsync();

            string existingContent = string.Empty;
            var download = await fileClient.DownloadAsync();
            using (var reader = new StreamReader(download.Value.Content))
            {
                existingContent = await reader.ReadToEndAsync();
            }

            var newContent = existingContent + $"{DateTimeOffset.UtcNow:u} - {logMessage}\n";
            var newContentBytes = System.Text.Encoding.UTF8.GetBytes(newContent);

            await fileClient.SetHttpHeadersAsync(new ShareFileSetHttpHeadersOptions
            {
                NewSize = newContentBytes.Length
            });
            using var stream = new MemoryStream(newContentBytes);
            await fileClient.UploadRangeAsync(new HttpRange(0, newContentBytes.Length), stream);
        }

        public async Task<string> ReadLogAsync()
        {
            var fileClient = await GetLogFileClientAsync();
            var download = await fileClient.DownloadAsync();
            using var reader = new StreamReader(download.Value.Content);
            return await reader.ReadToEndAsync();
        }
    }
}