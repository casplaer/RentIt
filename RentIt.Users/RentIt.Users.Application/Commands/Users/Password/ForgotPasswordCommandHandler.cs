﻿using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Password
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private const int _tokenSize = 64;
        private readonly IUserRepository _userRepository;
        private readonly IRepository<AccountToken> _accountTokenRepository;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IEmailSender _emailSender;
        private readonly IAccountTokenGenerator _accountTokenGenerator;
        private readonly ILinkGenerator _linkGenerator;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IRepository<AccountToken> accountTokenRepository,
            IEmailSender emailSender,
            IEmailNormalizer emailNormalizer,
            IAccountTokenGenerator accountTokenGenerator,
            ILinkGenerator linkGenerator)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _emailSender = emailSender;
            _emailNormalizer = emailNormalizer;
            _accountTokenGenerator = accountTokenGenerator;
            _linkGenerator = linkGenerator;
        }

        public async Task<bool> Handle(
            ForgotPasswordCommand request, 
            CancellationToken cancellationToken)
        {
            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);

            var user = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Пользователь с таким Email не найден.");
            }

            var token = _accountTokenGenerator.GenerateToken(_tokenSize);
            var accountToken = new AccountToken
            {
                TokenId = Guid.NewGuid(),
                UserId = user.UserId,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                TokenType = TokenType.PasswordReset
            };

            await _accountTokenRepository.AddAsync(accountToken, cancellationToken);
            await _accountTokenRepository.SaveChangesAsync(cancellationToken);

            var resetLink = _linkGenerator.GenerateResetPasswordLink(request.Email, token);
            var subject = "Восстановление пароля";
            var body = $"Если вы не запрашивали восстановление пароля просто проигнорируйте это сообщение. <br/> " +
                $"Для восстановления пароля перейдите по следующей ссылке: <a href='{resetLink}'>Восстановить пароль</a>";

            await _emailSender.SendEmailAsync(request.Email, subject, body, cancellationToken);

            return true;
        }
    }
}