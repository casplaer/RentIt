using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<AccountToken> _accountTokenRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IPasswordHasher _passwordHasher;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository, 
            IRepository<AccountToken> accountTokenRepository, 
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
        }

        public async Task<string> Handle(
            ResetPasswordCommand request, 
            CancellationToken cancellationToken)
        {
            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);

            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден.");
            }

            var tokens = await _accountTokenRepository.GetAllAsync(cancellationToken);

            var accountToken = tokens.FirstOrDefault(t =>
                t.TokenId == Guid.Parse(request.Token) &&
                t.TokenType == TokenType.PasswordReset);

            if (accountToken == null || 
                accountToken.UserId != user.UserId || 
                accountToken.Expiration < DateTime.UtcNow || 
                accountToken.TokenType != TokenType.PasswordReset)
            {
                throw new ArgumentException("Неверная или просроченная ссылка для восстановления пароля.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new ArgumentException("Пароли не совпадают.");
            }

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

            _accountTokenRepository.Delete(accountToken);
            await _accountTokenRepository.SaveChangesAsync(cancellationToken);

            await _userRepository.SaveChangesAsync(cancellationToken);

            return "Пароль успешно обновлён.";
        }
    }
}