using ReceiptSenderFunc.Models;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.SendGrid
{
    public interface IEmailCreator<TMessage> where TMessage : class
    {
        void AddAttachments(TMessage email, ICollection<AttachmentContent> attachments);

        TMessage? CreateEmail(SendReceiptRequest request, string senderAddress);
    }

}
