using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
