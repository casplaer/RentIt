using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Infrastructure.Services
{
    public class TokenCleanupService : ITokenCleanupService
    {
        private readonly IRepository<AccountToken> _accountTokenRepository;

        public TokenCleanupService(IRepository<AccountToken> accountTokenRepository)
        {
            _accountTokenRepository = accountTokenRepository;
        }

        public async Task CleanExpiredConfirmationTokensAsync(CancellationToken cancellationToken)
        {
            var tokens = await _accountTokenRepository.GetAllAsync(cancellationToken);

            var expiredTokens = tokens.Where(
                                        t => t.Expiration < DateTime.UtcNow && 
                                        t.TokenType == TokenType.Confirmation)
                                        .ToList();

            if (expiredTokens.Any())
            {
                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task CleanExpiredResetTokensAsync(CancellationToken cancellationToken)
        {
            var tokens = await _accountTokenRepository.GetAllAsync(cancellationToken);

            var expiredTokens = tokens.Where(
                                        t => t.Expiration < DateTime.UtcNow && 
                                        t.TokenType == TokenType.PasswordReset)
                                        .ToList();

            if (expiredTokens.Any())
            {
                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}