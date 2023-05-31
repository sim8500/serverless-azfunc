using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Models
{
    public record SendReceiptRequestDto(SendReceiptRequest Request, string ReferenceId, bool IgnoreNonMandatoryErrors = false);
}
