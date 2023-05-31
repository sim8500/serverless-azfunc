using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Models
{
    public class ReceiptDeliveryStatusEntity : ITableEntity
    {
        public ReceiptDeliveryStatusEntity()
        {
        }


        public ReceiptDeliveryStatusEntity(ReceiptDeliveryStatusEntry statusEntry)
        {
            RowKey = statusEntry.ReferenceId;
            Status = statusEntry.Status.ToString();
            MsgId = statusEntry.MsgId;
            Timestamp = statusEntry.Timestamp;
        }

        public string PartitionKey { get; set; } = PartitionKeyValue;

        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Status { get; set; }
        public string? MsgId { get; set;}

        public const string PartitionKeyValue = "receipt-delivery-statuses";

    }
}
