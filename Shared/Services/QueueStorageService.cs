using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace ABC_Inc_Project_CLD7112.Services
{
    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueServiceClient _serviceClient;

        public QueueStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];
            _serviceClient = new QueueServiceClient(connectionString);
        }

        private async Task<QueueClient> GetQueueClientAsync(string queueName)
        {
            var queueClient = _serviceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            return queueClient;
        }

        public async Task SendMessageAsync(string queueName, string messageText)
        {
            var queue = await GetQueueClientAsync(queueName);
            await queue.SendMessageAsync(messageText);
        }

        // TODO: ReceiveAndDeleteMessageAsync
        public async Task<string?> ReceiveAndDeleteMessageAsync(string queueName)
        {
            var queue = await GetQueueClientAsync(queueName);
            var response = await queue.ReceiveMessageAsync();

            if (response.Value == null)
            {
                return null;
            }

            var message = response.Value;
            var messageText = message.MessageText;

            await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);

            return messageText;
        }
    }
}