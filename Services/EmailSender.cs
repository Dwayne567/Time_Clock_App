using FT_TTMS_WebApplication.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace FT_TTMS_WebApplication.Services
{
    // Service for sending emails
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        // Send an email asynchronously
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(_emailSettings.SendGridApiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("dfraser@pcsfiber.com", "FiberTrak Time Clock"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            await client.SendEmailAsync(msg);
        }
    }
}