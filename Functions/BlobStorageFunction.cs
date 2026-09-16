using ABC_Inc_Project_CLD7112.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABC_Inc_Project_CLD7112.Functions
{
    // POST /api/files  -> upload a file to Blob Storage
    //                     send as multipart/form-data with a file field (any field name; the
    //                     first uploaded file is used)
    public class BlobStorageFunction
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<BlobStorageFunction> _logger;

        public BlobStorageFunction(IBlobStorageService blobStorageService, ILogger<BlobStorageFunction> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        [Function("BlobStorageFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "files")] HttpRequest req)
        {
            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Send a multipart/form-data request with a file attached.");
            }

            var form = await req.ReadFormAsync();
            if (form.Files.Count == 0)
            {
                return new BadRequestObjectResult("No file was found in the request. Attach a file under any form field name.");
            }

            var file = form.Files[0];
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            await using var stream = file.OpenReadStream();
            var url = await _blobStorageService.UploadImageAsync(stream, blobName, file.ContentType);

            _logger.LogInformation("Uploaded {BlobName} to Blob Storage.", blobName);

            return new OkObjectResult(new { fileName = file.FileName, blobName, url });
        }
    }
}
