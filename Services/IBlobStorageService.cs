namespace ABC_Inc_Project_CLD7112.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteImageAsync(string fileName);
    }
}
