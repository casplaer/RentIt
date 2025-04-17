using FluentValidation;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountTokenRepository _accountTokenRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<ResetPasswordCommand> _resetPasswordCommandValidator;
        private readonly ILogger _logger;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IAccountTokenRepository accountTokenRepository,
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer,
            IValidator<ResetPasswordCommand> resetPasswordCommandValidator,
            ILogger logger)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
            _resetPasswordCommandValidator = resetPasswordCommandValidator;
            _logger = logger;
        }

        public async Task<bool> Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Запрос на сброс пароля для email: {Email}", request.Email);

            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);

            if (user == null)
            {
                _logger.Warning("Пользователь с email {Email} не найден", request.Email);
                throw new NotFoundException("Пользователь не найден.");
            }

            var accountToken = await _accountTokenRepository.GetTokenAsync(
                request.Token,
                TokenType.PasswordReset,
                cancellationToken);

            bool isInvalidPasswordResetToken = accountToken == null ||
                                               accountToken.UserId != user.UserId ||
                                               accountToken.Expiration < DateTime.UtcNow ||
                                               accountToken.TokenType != TokenType.PasswordReset;

            if (isInvalidPasswordResetToken)
            {
                _logger.Warning("Неверный или просроченный токен сброса пароля для email: {Email}", request.Email);

                throw new ArgumentException("Неверная или просроченная ссылка для восстановления пароля.");
            }

            _logger.Information("Валидация команды сброса пароля для email: {Email}", request.Email);

            await _resetPasswordCommandValidator.ValidateAndThrowAsync(request, cancellationToken);

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Пароль успешно обновлён для пользователя с email: {Email}", request.Email);

            return true;
        }
    }
}
