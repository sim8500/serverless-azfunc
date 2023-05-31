using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.QueueHelpers
{
    public interface IMessageRedirectHandler
    {
        Task<bool> RedirectMessageIfApplicableAsync(string msgId, string message, int dequeueCount);
    }
}
