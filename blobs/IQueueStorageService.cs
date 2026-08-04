namespace ABC_Inc_Project_CLD7112.Services
{
    public interface IQueueStorageService
    {
        Task SendMessageAsync(string queueName, string messageText);
        Task<string?> ReceiveAndDeleteMessageAsync(string queueName);
    }
}