using RentIt.Users.Core.Entities;

namespace RentIt.Users.Core.Interfaces.Repositories
{
    public interface IAccountTokenRepository : IRepository<AccountToken>
    {
        Task<IEnumerable<AccountToken>> GetExpiredConfirmationTokensAsync(CancellationToken cancellationToken);
        Task<IEnumerable<AccountToken>> GetExpiredResetTokensAsync(CancellationToken cancellationToken);
    }
}
