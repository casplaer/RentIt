using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Interfaces
{
    public interface IJwtProvider
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User? user);
        Task RevokeAccessTokenAsync(string accessToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task<string> GetStoredTokenAsync(string token);
    }
}
