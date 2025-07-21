namespace Booking.Infrastructure.ExternalService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
    }
}
