using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Config
{
    public record QueueConfig
    {
        public required string MainQueueName { get; init; }
        public required string RetryQueueName { get; init; }
    }
}
