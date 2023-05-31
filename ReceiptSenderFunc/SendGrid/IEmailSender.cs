namespace ReceiptSenderFunc.SendGrid
{
    public interface IEmailSender<TMessage> where TMessage : class
    {
        Task<string?> SendEmailAsync(TMessage email);
    }
}
