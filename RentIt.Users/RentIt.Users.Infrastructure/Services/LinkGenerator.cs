using RentIt.Users.Application.Interfaces;

namespace RentIt.Users.Infrastructure.Services
{
    public class LinkGenerator : ILinkGenerator
    {
        private const string ConfirmationBaseUrl = "https://localhost:7108/users/confirm";
        private const string ResetPasswordBaseUrl = "https://renit.com/reset-password";

        public string GenerateConfirmationLink(Guid userId, string token)
        {
            return $"{ConfirmationBaseUrl}?userId={userId}&token={token}";
        }

        public string GenerateResetPasswordLink(string email, string token)
        {
            return $"{ResetPasswordBaseUrl}?email={email}&token={token}";
        }
    }
}