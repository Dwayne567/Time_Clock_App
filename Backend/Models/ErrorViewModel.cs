namespace Timeclock_WebApplication.Models
{
    // Model for error view
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
