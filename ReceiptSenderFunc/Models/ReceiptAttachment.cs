using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReceiptSenderFunc.Enums;

namespace ReceiptSenderFunc.Models
{
    public record ReceiptAttachment(
        string FileName,
        string FileUrl,
        DocumentCategory Category
        );
}
