using Hangfire.Logging;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Account
{
    public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountTokenRepository _accountTokenRepository;
        private readonly ILogger _logger;

        public ConfirmAccountCommandHandler(
            IUserRepository userRepository,
            IAccountTokenRepository accountTokenRepository)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _logger = Log.ForContext<ConfirmAccountCommandHandler>();
        }

        public async Task<bool> Handle(
            ConfirmAccountCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начата обработка подтверждения аккаунта для пользователя с ID: {UserId}", request.UserId);

            var tokenEntity = await _accountTokenRepository.GetTokenAsync(
                request.UserId,
                request.Token,
                TokenType.Confirmation,
                cancellationToken
            );

            if (tokenEntity == null)
            {
                _logger.Warning("Токен подтверждения не найден для пользователя {UserId}", request.UserId);

                throw new NotFoundException("Неверная или просроченная ссылка для подтверждения аккаунта.");
            }

            if (tokenEntity.Expiration < DateTime.UtcNow)
            {
                _logger.Warning("Токен подтверждения истёк для пользователя {UserId}", request.UserId);

                throw new NotFoundException("Неверная или просроченная ссылка для подтверждения аккаунта.");
            }

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.Error("Пользователь с ID {UserId} не найден при попытке подтверждения аккаунта", request.UserId);

                throw new NotFoundException("Пользователь не найден.");
            }

            user.Status = UserStatus.Active;
            _accountTokenRepository.Delete(tokenEntity);

            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Аккаунт пользователя с ID {UserId} успешно подтверждён", request.UserId);

            return true;
        }
    }
}
