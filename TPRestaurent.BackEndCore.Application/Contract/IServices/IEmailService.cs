namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IEmailService
    {
        public void SendEmail(string recipient, string subject, string body);
    }
}