using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Infrastructure.Services
{
    public class EmailNormalizer : IEmailNormalizer
    {
        public string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            return email.Trim().ToLowerInvariant();
        }
    }
}
