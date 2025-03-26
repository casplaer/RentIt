using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Services
{
    public class TokenCleanupService : ITokenCleanupService
    {
        private readonly IAccountTokenRepository _accountTokenRepository;

        public TokenCleanupService(IAccountTokenRepository accountTokenRepository)
        {
            _accountTokenRepository = accountTokenRepository;
        }

        public async Task CleanExpiredConfirmationTokensAsync(CancellationToken cancellationToken)
        {
            var expiredTokens = await _accountTokenRepository.GetExpiredConfirmationTokensAsync(cancellationToken);

            if (expiredTokens.Any())
            {
                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanExpiredResetTokensAsync(CancellationToken cancellationToken)
        {
            var expiredTokens = await _accountTokenRepository.GetExpiredConfirmationTokensAsync(cancellationToken);

            if (expiredTokens.Any())
            {
                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}