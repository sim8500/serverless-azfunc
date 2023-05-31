using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Models
{
    public record ReceiptDeliveryStatusEntry(string ReferenceId, string Status, string? MsgId, DateTime Timestamp);
}
