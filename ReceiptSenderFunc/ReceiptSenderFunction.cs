using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ReceiptSenderFunc.Models;
using ReceiptSenderFunc.QueueHelpers;
using ReceiptSenderFunc.Workflows;
using System.Text.Json;

namespace ReceiptSenderFunc
{
    public class ReceiptSenderFunction
    {
        public ReceiptSenderFunction(
            IReceiptSenderWorkflow receiptSenderWorkflow,
            IMessageRedirectHandler redirectHandler,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReceiptSenderFunction>();
            _receiptSenderWorkflow = receiptSenderWorkflow;
            _redirectHandler = redirectHandler;
        }

        [Function("ReceiptSender")]
        public async Task RunReceiptSenderAsync([QueueTrigger("%QueueConfig:MainQueueName%")] string queueItem,
            string id,
            int dequeueCount)
        {
            _logger.LogInformation($"C# Queue trigger ReceiptSender func invoked: {queueItem}");

            var req = JsonSerializer.Deserialize<SendReceiptRequestDto>(queueItem);

            try
            {
                await _receiptSenderWorkflow.RunAsync(req!);
            }
            catch
            {
                if (await _redirectHandler.RedirectMessageIfApplicableAsync(id, queueItem, dequeueCount))
                {
                    return;
                }

                throw;
            }
        }

        [Function("ReceiptSender_RelaxedRules")]
        public async Task RunReceiptSenderRelaxedAsync([QueueTrigger("%QueueConfig:RetryQueueName%")] string queueItem)
        {
            _logger.LogInformation($"C# Queue trigger ReceiptSender_RelaxedRules func invoked: {queueItem}");

            var req = JsonSerializer.Deserialize<SendReceiptRequestDto>(queueItem);
            
            await _receiptSenderWorkflow.RunAsync(req! with { IgnoreNonMandatoryErrors = true });
        }

        private readonly ILogger _logger;
        private readonly IReceiptSenderWorkflow _receiptSenderWorkflow;
        private readonly IMessageRedirectHandler _redirectHandler;
    }

}
