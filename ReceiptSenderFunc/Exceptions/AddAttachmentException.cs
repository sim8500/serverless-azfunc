using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Exceptions
{
    public class AddAttachmentException : Exception
    {
        public AddAttachmentException(string message) : base(message)
        {
        }
    }
}
