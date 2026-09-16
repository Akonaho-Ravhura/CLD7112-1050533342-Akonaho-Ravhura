namespace ABC_Inc_Project_CLD7112.Services
{
    public interface IFileStorageService
    {
        Task WriteLogAsync(string logMessage);
        Task<string> ReadLogAsync();
    }
}