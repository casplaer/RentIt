using Microsoft.Extensions.Options;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Infrastructure.Options;
using System.Net.Mail;
using System.Net;

namespace RentIt.Users.Infrastructure.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;

        public EmailSender(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions.Value;
        }

        public async Task SendEmailAsync(
            string to, 
            string subject, 
            string htmlMessage, 
            CancellationToken cancellationToken)
        {
            using (var smtpClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpOptions.User, _smtpOptions.Password);
                smtpClient.EnableSsl = _smtpOptions.EnableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpOptions.From),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
        }
    }
}
