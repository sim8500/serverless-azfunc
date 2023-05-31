using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Models
{
    public class AttachmentContent
    {
        public required string FileName { get; set; }
        public string? ContentType { get; set; }
        public string? Base64Content { get; set; }
    }
}
