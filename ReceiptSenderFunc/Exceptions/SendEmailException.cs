using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Exceptions
{
    public class SendEmailException : Exception
    { 

        public SendEmailException(string message) : base(message)
        {
        }
    }
}
