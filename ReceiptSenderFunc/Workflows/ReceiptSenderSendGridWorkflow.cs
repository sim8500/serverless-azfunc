using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReceiptSenderFunc.Config;
using ReceiptSenderFunc.DeliveryStatus;
using ReceiptSenderFunc.Enums;
using ReceiptSenderFunc.Exceptions;
using ReceiptSenderFunc.Models;
using ReceiptSenderFunc.SendGrid;
using SendGrid.Helpers.Mail;
using static Grpc.Core.Metadata;

namespace ReceiptSenderFunc.Workflows
{
    public class ReceiptSenderSendGridWorkflow : IReceiptSenderWorkflow
    {
        public ReceiptSenderSendGridWorkflow(IEmailCreator<SendGridMessage> emailCreator,
                                             IEmailSender<SendGridMessage> emailSender,
                                             IReceiptDeliveryStatusRepository deliveryStatusRepository,
                                            IOptions<ReceiptSenderConfig> config,
                                            HttpClient httpClient,
                                            ILogger<ReceiptSenderSendGridWorkflow> logger)
        {
            _emailCreator = emailCreator;
            _emailSender = emailSender;
            _deliveryStatusRepository = deliveryStatusRepository;
            _config = config.Value;
            _httpClient = new FlurlClient(httpClient);
            _logger = logger;
        }

        public async Task RunAsync(SendReceiptRequestDto request)
        {
            var statusEntry = await _deliveryStatusRepository.GetAsync(request.ReferenceId);

            if (statusEntry == null || IsDeliveryStatusValidForProcessing(statusEntry.Status))
            {
                var email = _emailCreator.CreateEmail(request.Request, _config.SenderAddress);

                if (email != null)
                {
                    if (statusEntry == null)
                    {
                        statusEntry = new ReceiptDeliveryStatusEntry(
                            request.ReferenceId,
                            ReceiptDeliveryStatus.Created.ToString(),
                            null,
                            DateTime.UtcNow);
                        await _deliveryStatusRepository.UpsertAsync(statusEntry);
                    }

                    if (request.Request.Attachments?.Any() ?? false)
                    {
                        await HandleAttachments(email, request.Request.Attachments, statusEntry, request.IgnoreNonMandatoryErrors);
                    }

                    await HandleEmailSendout(request, email, statusEntry);
                }
                else
                {
                    _logger.LogError($"Unable to prepare SendGrid request for " +
                        $"Template = {request.Request.Template}, Lang = {request.Request.LanguageVersion}," +
                        $" (ReferenceId = {request.ReferenceId}).");

                    throw new SendEmailException($"Failed to create email for RefId = {request.ReferenceId}");
                }
            }
            else
            {
                _logger.LogWarning($"Received SendReceipt request for RefId = {request.ReferenceId} with status set to: {statusEntry.Status}.");
            }
        }

        private bool IsDeliveryStatusValidForProcessing(string deliveryStatus)
        {
            return !ReceiptDeliveryStatus.Sent.ToString().Equals(deliveryStatus, StringComparison.OrdinalIgnoreCase) &&
                    !ReceiptDeliveryStatus.SendingScheduled.ToString().Equals(deliveryStatus, StringComparison.OrdinalIgnoreCase);
        }

        private async Task HandleAttachments(SendGridMessage email, ICollection<ReceiptAttachment> attachments, ReceiptDeliveryStatusEntry statusEntry, bool ignoreNonMandatoryErrors)
        {
            IEnumerable<ReceiptAttachment>? toBeAttachedMandatory = null;
            IEnumerable<ReceiptAttachment>? toBeAttachedOptional = null;

            if (attachments.Any())
            {
                toBeAttachedMandatory = attachments.Where(a => a.Category == DocumentCategory.Mandatory);
                toBeAttachedOptional = attachments.Where(a => a.Category == DocumentCategory.Optional);

                if (toBeAttachedMandatory.Any())
                {
                    _logger.LogInformation($"Received {toBeAttachedMandatory.Count()} attachments " +
                                        $"from {DocumentCategory.Mandatory} category.");

                   var failuresCount = await HandleAttachmentsSubset(toBeAttachedMandatory, email);
                  
                   if (failuresCount > 0)
                   {
                        statusEntry = statusEntry with { Status = ReceiptDeliveryStatus.Failed.ToString() };

                        await _deliveryStatusRepository.UpsertAsync(statusEntry);

                        throw new AddAttachmentException($"Failed to add {failuresCount} of {DocumentCategory.Mandatory} category.");
                   }
                }

                if (toBeAttachedOptional.Any())
                {
                    _logger.LogInformation($"Received {toBeAttachedOptional.Count()} attachments " +
                                        $"from {DocumentCategory.Optional} category.");

                  var failuresCount = await HandleAttachmentsSubset(toBeAttachedOptional, email);
                 
                  if (failuresCount > 0 && !ignoreNonMandatoryErrors)
                  {
                      throw new AddAttachmentException($"Failed to add {failuresCount} of {DocumentCategory.Optional} category.");
                  }
                  else if (failuresCount > 0)
                  {
                      _logger.LogError($"Failed to add {failuresCount} of {DocumentCategory.Optional} category.");
                  }
                }
            }
        }

        private async Task HandleEmailSendout(SendReceiptRequestDto request, SendGridMessage email, ReceiptDeliveryStatusEntry statusEntry)
        {
            try
            {
                var res = await _emailSender.SendEmailAsync(email);

                statusEntry = statusEntry with { MsgId = res, Status = ReceiptDeliveryStatus.SendingScheduled.ToString() };

                await _deliveryStatusRepository.UpsertAsync(statusEntry);
            }
            catch (SendEmailException ex)
            {
                statusEntry = statusEntry with { Status = ReceiptDeliveryStatus.Failed.ToString() };

                await _deliveryStatusRepository.UpsertAsync(statusEntry);

                throw ex;
            }
        }

          private async Task<int> HandleAttachmentsSubset(IEnumerable<ReceiptAttachment> toBeAttached, SendGridMessage email)
          {
            var attachTasks = toBeAttached.Select(async att =>
            {
                try
                {
                    var stream = await (new Flurl.Url(att.FileUrl!).WithClient(_httpClient!).GetStreamAsync());
                    var result = new byte[stream.Length];
                    await stream.ReadAsync(result, 0, (int)stream.Length);
                    return new AttachmentContent
                    {
                        FileName = att.FileName,
                        Base64Content = Convert.ToBase64String(result)
                    };
                }
                catch
                {
                    return null;
                }
            });
                
              var downloadResults = await Task.WhenAll(attachTasks);
        
              var successfulDownloads = downloadResults.Where(r => r != null).ToList();
        
              if (successfulDownloads.Count() == toBeAttached.Count())
              {
                  _emailCreator.AddAttachments(email, successfulDownloads!);
              }
        
              return downloadResults.Count() - successfulDownloads.Count();
          }

        private readonly IEmailCreator<SendGridMessage> _emailCreator;
        private readonly IEmailSender<SendGridMessage> _emailSender;
        private readonly IReceiptDeliveryStatusRepository _deliveryStatusRepository;
        private readonly IFlurlClient _httpClient;
        private readonly ReceiptSenderConfig _config;
        private readonly ILogger _logger;
    }
}
