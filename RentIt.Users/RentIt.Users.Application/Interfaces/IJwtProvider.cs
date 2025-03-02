using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User? user);
        Task RevokeAccessTokenAsync(string accessToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<string> GetStoredTokenAsync(string token);
    }
}
