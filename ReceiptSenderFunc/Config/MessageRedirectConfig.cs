using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Config
{
    public record MessageRedirectConfig
    {
        public required string TargetQueueName { get; init; }
        public required int MaxRetryCount { get; init; }
    }
}
