using System.Text.Json;
using ABC_Inc_Project_CLD7112.Models;
using ABC_Inc_Project_CLD7112.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABC_Inc_Project_CLD7112.Functions
{
    // POST /api/transactions  -> write a transaction message onto the queue
    //                            body: { "orderId": "...", "status": "Pending" }
    // GET  /api/transactions  -> read (and remove) the next transaction message off the queue
    //
    // Uses the same "order-processing" queue the website's OrderController already writes to
    // (on order placement and status changes), so this function reads real messages produced
    // by the site rather than a separate, disconnected queue.
    public class QueueStorageFunction
    {
        private const string QueueName = "order-processing";

        private readonly IQueueStorageService _queueStorageService;
        private readonly ILogger<QueueStorageFunction> _logger;

        public QueueStorageFunction(IQueueStorageService queueStorageService, ILogger<QueueStorageFunction> logger)
        {
            _queueStorageService = queueStorageService;
            _logger = logger;
        }

        [Function("QueueStorageFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "transactions")] HttpRequest req)
        {
            if (HttpMethods.IsPost(req.Method))
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();

                var transaction = string.IsNullOrWhiteSpace(body)
                    ? null
                    : JsonSerializer.Deserialize<OrderMessage>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (transaction == null || string.IsNullOrWhiteSpace(transaction.OrderId))
                {
                    return new BadRequestObjectResult(
                        "Request body must include at least an orderId, e.g. { \"orderId\": \"123\", \"status\": \"Pending\" }.");
                }

                transaction.Timestamp = DateTimeOffset.UtcNow;
                await _queueStorageService.SendMessageAsync(QueueName, JsonSerializer.Serialize(transaction));

                _logger.LogInformation("Sent transaction message for order {OrderId} to Queue Storage.", transaction.OrderId);

                return new OkObjectResult(transaction);
            }

            var messageText = await _queueStorageService.ReceiveAndDeleteMessageAsync(QueueName);
            if (messageText == null)
            {
                return new OkObjectResult(new { message = "No transaction messages waiting in the queue." });
            }

            var received = JsonSerializer.Deserialize<OrderMessage>(messageText);
            return new OkObjectResult(received);
        }
    }
}
