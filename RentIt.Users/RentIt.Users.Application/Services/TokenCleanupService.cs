using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Services
{
    public class TokenCleanupService : ITokenCleanupService
    {
        private readonly IAccountTokenRepository _accountTokenRepository;
        private readonly ILogger _logger;

        public TokenCleanupService(
            IAccountTokenRepository accountTokenRepository,
            ILogger logger)
        {
            _accountTokenRepository = accountTokenRepository;
            _logger = logger;
        }

        public async Task CleanExpiredConfirmationTokensAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Начинаем очистку просроченных токенов подтверждения.");

            var expiredTokens = await _accountTokenRepository.GetExpiredConfirmationTokensAsync(cancellationToken);

            if (expiredTokens.Any())
            {
                _logger.Information("Найдены {ExpiredTokensCount} просроченных токенов подтверждения, удаляем их.", expiredTokens.Count());

                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);

                _logger.Information("Просроченные токены подтверждения удалены.");
            }
            else
            {
                _logger.Information("Просроченные токены подтверждения не найдены.");
            }
        }

        public async Task CleanExpiredResetTokensAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Начинаем очистку просроченных токенов для сброса пароля.");

            var expiredTokens = await _accountTokenRepository.GetExpiredResetTokensAsync(cancellationToken);

            if (expiredTokens.Any())
            {
                _logger.Information("Найдены {ExpiredTokensCount} просроченных токенов сброса пароля, удаляем их.", expiredTokens.Count());

                _accountTokenRepository.RemoveRange(expiredTokens);
                await _accountTokenRepository.SaveChangesAsync(cancellationToken);

                _logger.Information("Просроченные токены сброса пароля удалены.");
            }
            else
            {
                _logger.Information("Просроченные токены сброса пароля не найдены.");
            }
        }
    }
}
