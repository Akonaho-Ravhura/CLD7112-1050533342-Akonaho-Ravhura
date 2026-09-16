using ABC_Inc_Project_CLD7112.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABC_Inc_Project_CLD7112.Functions
{
    // POST /api/logs  -> append a line to the Azure Files log (body: raw text to append)
    // GET  /api/logs  -> read back the full log file
    public class FileStorageFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<FileStorageFunction> _logger;

        public FileStorageFunction(IFileStorageService fileStorageService, ILogger<FileStorageFunction> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [Function("FileStorageFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "logs")] HttpRequest req)
        {
            if (HttpMethods.IsPost(req.Method))
            {
                using var reader = new StreamReader(req.Body);
                var message = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(message))
                {
                    return new BadRequestObjectResult("Request body must contain the text to append to the log.");
                }

                await _fileStorageService.WriteLogAsync(message);
                _logger.LogInformation("Appended entry to the Azure Files log.");

                return new OkObjectResult(new { appended = message });
            }

            var log = await _fileStorageService.ReadLogAsync();
            return new OkObjectResult(new { log });
        }
    }
}
