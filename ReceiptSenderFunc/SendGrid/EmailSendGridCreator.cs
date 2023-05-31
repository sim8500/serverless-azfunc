using ReceiptSenderFunc.Models;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.SendGrid
{
    public class EmailSendGridCreator : IEmailCreator<SendGridMessage>
    {
        public SendGridMessage? CreateEmail(SendReceiptRequest request, string senderAddress)
        {
            SendGridMessage? result = null;

            var templateId = GetTemplate(request);
            if (!string.IsNullOrEmpty(templateId))
            {
                var paramsDto = new
                {
                    CustomerNumberField = request.CustomerNumber,
                    OrderNumberField = request.OrderRefNumber,
                    OrderUrlField = request.OrderUrl,
                    request.Subject,
                };

                var sender = PrepareSender(senderAddress);

                result = MailHelper.CreateSingleTemplateEmail(sender,
                                                            new EmailAddress(request.EmailAddress),
                                                            templateId,
                                                            paramsDto);
                result.Subject = request.Subject;
            }


            return result;
        }

        public void AddAttachments(SendGridMessage email, ICollection<AttachmentContent> attachments)
        {
            foreach(var att in attachments)
            {
                email.AddAttachment(att.FileName, att.Base64Content, att.ContentType);
            }
        }


        private static EmailAddress PrepareSender(string senderAddress, string? senderName = null)
        {
            return (!string.IsNullOrEmpty(senderName)) ?
                     new EmailAddress(senderAddress, senderName) :
                     new EmailAddress(senderAddress);
        }

        private static string GetTemplate(SendReceiptRequest request) => "d-8f5777f64e2d4917bfd93265b9b22908";


    }
}
