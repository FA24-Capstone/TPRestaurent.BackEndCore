using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using MimeKit;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class EmailService : GenericBackendService, IEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly BackEndLogger _logger;

        public EmailService(BackEndLogger logger, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
            _emailConfiguration = Resolve<EmailConfiguration>()!;
        }

        public void SendEmail(string recipient, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Thiên Phú Restaurent", _emailConfiguration.User));
                message.To.Add(new MailboxAddress("Khách hàng", recipient));
                message.Subject = subject;
                message.Importance = MessageImportance.High;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    client.Authenticate(_emailConfiguration.User, _emailConfiguration.ApplicationPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
        }


        public void SendEmailWithFiles(string recipient, string subject, string body, List<IFormFile> files)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Thiên Phú Restaurent", _emailConfiguration.User));
                message.To.Add(new MailboxAddress("Khách hàng", recipient));
                message.Subject = subject;
                message.Importance = MessageImportance.High;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;

                // Attach files
                foreach (var file in files)
                {
                    var attachment = new MimePart(file.ContentType)
                    {
                        Content = new MimeContent(file.OpenReadStream()),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = file.FileName
                    };
                    bodyBuilder.Attachments.Add(attachment);
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    client.Authenticate(_emailConfiguration.User, _emailConfiguration.ApplicationPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
        }

        public void SendEmailWithFiles(string recipient, string subject, string body, Dictionary<string, List<IFormFile>> files)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Thiên Phú Restaurent", _emailConfiguration.User));
                message.To.Add(new MailboxAddress("Khách hàng", recipient));
                message.Subject = subject;
                message.Importance = MessageImportance.High;

                var bodyBuilder = new BodyBuilder();
                var emailBody = new System.Text.StringBuilder(body);

                // Attach files
                foreach (var rank in files)
                {
                    string rankName = rank.Key;
                    emailBody.AppendLine($"<h3>{rankName}</h3><ul>");
                    foreach (var file in rank.Value)
                    {
                        var attachment = new MimePart(file.ContentType)
                        {
                            Content = new MimeContent(file.OpenReadStream()),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = $"{rankName}_{file.FileName}"
                        };
                        bodyBuilder.Attachments.Add(attachment);
                    }
                    emailBody.AppendLine("</ul>");
                }

                bodyBuilder.HtmlBody = emailBody.ToString();
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    client.Authenticate(_emailConfiguration.User, _emailConfiguration.ApplicationPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
        }
    }
}
