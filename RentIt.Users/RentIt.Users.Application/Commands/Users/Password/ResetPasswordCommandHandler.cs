using FluentValidation;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<AccountToken> _accountTokenRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<ResetPasswordCommand> _resetPasswordCommandValidator;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository, 
            IRepository<AccountToken> accountTokenRepository, 
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer,
            IValidator<ResetPasswordCommand> resetPasswordCommandValidator)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
            _resetPasswordCommandValidator = resetPasswordCommandValidator;
        }

        public async Task<bool> Handle(
            ResetPasswordCommand request, 
            CancellationToken cancellationToken)
        {
            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);

            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            var accountToken = (await _accountTokenRepository.GetAllAsync(cancellationToken))
                .FirstOrDefault(t =>
                t.TokenId == Guid.Parse(request.Token) &&
                t.TokenType == TokenType.PasswordReset);

            bool isInvalidPasswordResetToken = accountToken == null ||
                                               accountToken.UserId != user.UserId ||
                                               accountToken.Expiration < DateTime.UtcNow ||
                                               accountToken.TokenType != TokenType.PasswordReset;

            if (isInvalidPasswordResetToken)
            {
                throw new ArgumentException("Неверная или просроченная ссылка для восстановления пароля.");
            }

            await _resetPasswordCommandValidator.ValidateAndThrowAsync(request, cancellationToken);

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

            await _userRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}