using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Config
{
    public record ReceiptSenderConfig
    {
        public required string SenderAddress { get; init; }

        public string? SenderName { get; init; }

        public bool HideTrackingToken { get; init; } = true;
    }
}
