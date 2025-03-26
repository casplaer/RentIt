using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;

namespace RentIt.Users.Core.Interfaces.Repositories
{
    public interface IAccountTokenRepository : IRepository<AccountToken>
    {
        Task<IEnumerable<AccountToken>> GetExpiredConfirmationTokensAsync(CancellationToken cancellationToken);
        Task<IEnumerable<AccountToken>> GetExpiredResetTokensAsync(CancellationToken cancellationToken);
        Task<AccountToken> GetTokenAsync(Guid userId, string token, TokenType type, CancellationToken cancellationToken);
    }
}
