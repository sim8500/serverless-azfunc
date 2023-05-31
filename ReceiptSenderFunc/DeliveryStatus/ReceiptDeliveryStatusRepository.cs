using Azure.Data.Tables;
using ReceiptSenderFunc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.DeliveryStatus
{
    public class ReceiptDeliveryStatusRepository : IReceiptDeliveryStatusRepository
    {
        public ReceiptDeliveryStatusRepository() 
        {
            _tableClient = new TableClient("UseDevelopmentStorage=true", "ReceiptDeliveryStatuses");
        }
        public async Task CreateAsync(ReceiptDeliveryStatusEntry item)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(new ReceiptDeliveryStatusEntity(item));
        }

        public async Task<ReceiptDeliveryStatusEntry?> GetAsync(string referenceId)
        {
            var result = await _tableClient.GetEntityIfExistsAsync<ReceiptDeliveryStatusEntity>(ReceiptDeliveryStatusEntity.PartitionKeyValue, referenceId);

            if (result.HasValue)
            {
                return new ReceiptDeliveryStatusEntry(
                    referenceId,
                    result.Value.Status,
                    result.Value.MsgId,
                    result.Value.Timestamp?.UtcDateTime ?? DateTime.UtcNow);
            }

            return null;
        }

        public Task<ReceiptDeliveryStatusEntry?> GetByMsgIdAsync(string msgId)
        {
            var result = _tableClient
                .Query<ReceiptDeliveryStatusEntity>(
                    x => x.PartitionKey == ReceiptDeliveryStatusEntity.PartitionKeyValue && x.MsgId == msgId)
                .FirstOrDefault();

            if (result != null)
            {
                return Task.FromResult(
                    new ReceiptDeliveryStatusEntry(
                        result.RowKey,
                        result.Status,
                        result.MsgId,
                        result.Timestamp?.UtcDateTime ?? DateTime.UtcNow));
            }

            return Task.FromResult<ReceiptDeliveryStatusEntry?>(null);
        }

        public async Task UpsertAsync(ReceiptDeliveryStatusEntry item)
        {
            await _tableClient.CreateIfNotExistsAsync();

            var result = await _tableClient.GetEntityIfExistsAsync<ReceiptDeliveryStatusEntity>(ReceiptDeliveryStatusEntity.PartitionKeyValue, item.ReferenceId);

            if (result.HasValue)
            {
                result.Value.Status = item.Status;
                result.Value.MsgId = item.MsgId;
                result.Value.Timestamp = item.Timestamp;

                await _tableClient.UpdateEntityAsync<ReceiptDeliveryStatusEntity>(
                    result.Value,
                    result.Value.ETag,
                    TableUpdateMode.Merge);
            }
            else
            {
                await _tableClient.AddEntityAsync(new ReceiptDeliveryStatusEntity(item));
            }
        }

        private readonly TableClient _tableClient;
    }
}
