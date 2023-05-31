using ReceiptSenderFunc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.DeliveryStatus
{
    public interface IReceiptDeliveryStatusRepository
    {
        Task CreateAsync(ReceiptDeliveryStatusEntry item);

        Task UpsertAsync(ReceiptDeliveryStatusEntry item);

        Task<ReceiptDeliveryStatusEntry?> GetAsync(string referenceId);

        Task<ReceiptDeliveryStatusEntry?> GetByMsgIdAsync(string msgId);
    }
}
