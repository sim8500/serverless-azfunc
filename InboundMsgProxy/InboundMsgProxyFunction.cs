using System;
using EDI.InboundMsgProxy.Function.Model;
using EDI.InboundMsgProxy.Services;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InboundMsgProxy
{
    public class InboundMsgProxyFunction
    {
        public InboundMsgProxyFunction(IInboundMsgProxyService inboundService)
        {
            _inboundService = inboundService;
        }


        [FunctionName("InboundMsgProxy")]
        public async Task Run([QueueTrigger("%SourceQueue%", Connection = "AzureWebJobsStorage")] string myQueueItem, string id, int dequeueCount, ILogger log)
        {
            log.LogInformation($"InboundMsgProxy function triggered with msg: {myQueueItem}");

            try
            {
                var msg = JsonSerializer.Deserialize<QueueMsgDto>(myQueueItem);
                var response = await _inboundService.Process(msg.TargetUrl, msg.Message);

                response.EnsureSuccessStatusCode();
            }
            catch (JsonException ex)
            {
                log.LogError($"Cannot parse incoming message: {myQueueItem} - details: {ex.Message}");

            }
            catch (Exception ex)
            {
                log.LogError($"Unknown error occurred: {ex.Message}");
                throw;
            }
        }

        private readonly IInboundMsgProxyService _inboundService;
    }


}
