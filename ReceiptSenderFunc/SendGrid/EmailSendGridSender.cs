using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReceiptSenderFunc.Exceptions;
using ReceiptSenderFunc.Models;
using ReceiptSenderFunc.SendGrid;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ReceiptSenderFunc.EmailSender.SendGrid
{
    public class EmailSendGridSender : IEmailSender<SendGridMessage>
    {
        public EmailSendGridSender(ISendGridClient sendGridClient, ILogger<EmailSendGridSender> logger)
        {
            _sendGridClient = sendGridClient;
            _logger = logger;
        }

       

        public async Task<string?> SendEmailAsync(SendGridMessage email)
        {
            var res = await _sendGridClient.SendEmailAsync(email);

            if (res.StatusCode != HttpStatusCode.OK && 
                res.StatusCode != HttpStatusCode.Accepted)
            {
                var content = await res.Body.ReadAsStringAsync();
                var msg = $"Email sendout failed with: {res.StatusCode} - {content}";

                _logger.LogError(msg);
                throw new SendEmailException(msg);
            }
            else
            {
                string message = $"Successfully scheduled email sendout for TemplateId = {email.TemplateId}";
                _logger.LogInformation(message);

                return res.Headers.GetValues("X-Message-Id")?.FirstOrDefault();
            }
        }

        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger _logger;
    }
}
