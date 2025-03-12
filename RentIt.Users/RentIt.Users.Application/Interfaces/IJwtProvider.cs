using RentIt.Users.Core.Entities;

namespace RentIt.Users.Application.Interfaces
{
    public interface IJwtProvider
    {
        Task<string> GenerateAccessTokenAsync(User user, CancellationToken cancellationToken);
        Task<string> GenerateRefreshTokenAsync(User? user, CancellationToken cancellationToken);
        Task RevokeAccessTokenAsync(string accessToken, CancellationToken cancellationToken);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<string> GetStoredTokenAsync(string token, CancellationToken cancellationToken);
    }
}
