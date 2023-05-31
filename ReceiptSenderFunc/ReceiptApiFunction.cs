using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ReceiptSenderFunc.Config;
using ReceiptSenderFunc.DeliveryStatus;
using ReceiptSenderFunc.Enums;
using ReceiptSenderFunc.Models;

namespace ReceiptSenderFunc
{
    public class ReceiptApiFunction
    {
        public ReceiptApiFunction(
            IReceiptDeliveryStatusRepository deliveryStatusRepository,
            IOptions<QueueConfig> queueConfig,
            ILoggerFactory loggerFactory)
        {     
           _queueConfig = queueConfig.Value ?? throw new ArgumentNullException(nameof(queueConfig)); 
            _queueClient = new QueueClient(
                "UseDevelopmentStorage=true", 
                _queueConfig.MainQueueName ?? throw new ArgumentNullException(nameof(QueueConfig.MainQueueName)));

            _deliveryStatusRepository = deliveryStatusRepository;
            _logger = loggerFactory.CreateLogger<ReceiptApiFunction>();
        }

        [Function("send-receipt")]
        public async Task<HttpResponseData> RunSend(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "receipts/:send")] HttpRequestData req)
        {

            var reqObj = await req.ReadFromJsonAsync<SendReceiptRequest>();

            if (reqObj != null)
            {
                var referenceId = Guid.NewGuid().ToString();

                await _queueClient.CreateIfNotExistsAsync();

                var base64MsgString = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(new SendReceiptRequestDto(
                            reqObj,
                            referenceId))));

                await _queueClient.SendMessageAsync(base64MsgString);

                _logger.LogInformation($"SendReceipt Request scheduled for processing. Ref-Id: {referenceId}");

                var response = req.CreateResponse(HttpStatusCode.Accepted);
                response.Headers.Add("x-ref-id", referenceId);

                return response;
            }

            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        [Function("get-status")]
        public async Task<HttpResponseData> RunGet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "receipts/{refid}/status")] HttpRequestData req, string refid)
        {
            var result = await _deliveryStatusRepository.GetAsync(refid);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            response.Body.Write(JsonSerializer.SerializeToUtf8Bytes(
                result,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

            return response;
        }

        [Function("sync-statuses")]
        public async Task<HttpResponseData> RunSync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "receipts/:sync")] HttpRequestData req)
        {
            string? requestBody = await req.ReadAsStringAsync();
            string bodyObj = $"{{\"events\" : {requestBody}}}";

            var eventData = JsonDocument.Parse(bodyObj);

            foreach (var ev in eventData.RootElement.GetProperty("events").EnumerateArray())
            {
                var msgid = ExtractMsgId(ev.GetProperty("sg_message_id").GetString());

                if (!string.IsNullOrEmpty(msgid))
                {
                    await ProcessEvent(msgid, ev.GetProperty("event").GetString());
                }
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static string? ExtractMsgId(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                var dotIndex = source.IndexOf('.');
                if (dotIndex > 0)
                {
                    return source[..dotIndex];
                }
            }

            return null;
        }

        private async Task ProcessEvent(string msgid, string evtype)
        {
            _logger.LogInformation($"[sync-statuses] Received event: '{evtype}' for msgId = {msgid}");

            if (IsDeliveredEvent(evtype))
            {
                var item = await _deliveryStatusRepository.GetByMsgIdAsync(msgid);
                if (item != null)
                {
                    await _deliveryStatusRepository.UpsertAsync(item with { Status = ReceiptDeliveryStatus.Sent.ToString() });
                }
            }
        }

        private bool IsDeliveredEvent(string eventType)
        {
            return "delivered".Equals(eventType, StringComparison.OrdinalIgnoreCase);
        }

        private readonly ILogger _logger;
        private readonly QueueClient _queueClient;
        private readonly IReceiptDeliveryStatusRepository _deliveryStatusRepository;
        private readonly QueueConfig _queueConfig;

    }
}
