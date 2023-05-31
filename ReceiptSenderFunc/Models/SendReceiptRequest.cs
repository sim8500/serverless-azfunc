using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceiptSenderFunc.Enums;

namespace ReceiptSenderFunc.Models
{
    public record SendReceiptRequest
    {
        public required string CustomerNumber { get; init; }
        public required string EmailAddress { get; init; }
        public required EmailTemplate Template { get; init; }
        public required TemplateLanguageVersion LanguageVersion { get; init; }
        
        public string? OrderRefNumber { get; init; }
        public string? OrderUrl { get; init; }
        public string? Subject { get; init; }
        public ICollection<ReceiptAttachment>? Attachments { get; init; }
    }
}
