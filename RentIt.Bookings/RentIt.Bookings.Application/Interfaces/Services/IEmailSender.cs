namespace RentIt.Bookings.Application.Interfaces.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage, CancellationToken cancellationToken);
    }
}
