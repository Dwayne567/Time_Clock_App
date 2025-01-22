using System.Threading.Tasks;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for email sender service
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}