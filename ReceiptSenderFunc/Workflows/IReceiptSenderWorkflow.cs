using ReceiptSenderFunc.Models;
using System.Threading.Tasks;

namespace ReceiptSenderFunc.Workflows
{
    public interface IReceiptSenderWorkflow
    {
        Task RunAsync(SendReceiptRequestDto request);
    }
}
