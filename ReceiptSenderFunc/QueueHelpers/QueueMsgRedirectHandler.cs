using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReceiptSenderFunc.Config;
using System.Text;

namespace ReceiptSenderFunc.QueueHelpers
{
    public class QueueMsgRedirectHandler : IMessageRedirectHandler
    {
        public QueueMsgRedirectHandler(
            IOptions<MessageRedirectConfig> config,
            ILogger<QueueMsgRedirectHandler> logger)
        {
            _config = config?.Value ?? throw new ArgumentException(nameof(config));

            _queueClient = new QueueClient("UseDevelopmentStorage=true", _config.TargetQueueName);

            _logger = logger;
        }

        public async Task<bool> RedirectMessageIfApplicableAsync(string msgId, string message, int dequeueCount)
        {
            if(dequeueCount >= _config.MaxRetryCount)
            {
                await _queueClient.CreateIfNotExistsAsync();

                var base64MsgString = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(message));

                await _queueClient.SendMessageAsync(base64MsgString);
                _logger.LogWarning($"Message (Id = {msgId}) redirected to {_config.TargetQueueName} queue.");
                return true;
            }

            return false;
        }

        private readonly ILogger _logger;
        private readonly QueueClient _queueClient;
        private readonly MessageRedirectConfig _config;
    }
}
