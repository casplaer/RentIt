using RentIt.Users.Application.Interfaces;
using System.Security.Cryptography;

namespace RentIt.Users.Infrastructure.Services
{
    public class AccountTokenGenerator : IAccountTokenGenerator
    {
        public string GenerateToken(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create()) 
            {
                var byteArray = new byte[length];
                rng.GetBytes(byteArray);

                return Convert.ToBase64String(byteArray).Substring(0, length);
            }
        }
    }
}
