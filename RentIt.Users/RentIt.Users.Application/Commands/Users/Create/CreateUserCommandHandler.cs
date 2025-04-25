using AutoMapper;
using Hangfire;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using Serilog;

namespace RentIt.Users.Application.Commands.Users.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private const int TokenSize = 64;

        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IAccountTokenRepository _accountTokenRepository;
        private readonly IEmailSender _emailSender;
        private readonly IAccountTokenGenerator _accountTokenGenerator;
        private readonly ILinkGenerator _linkGenerator;
        private readonly ILogger _logger;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer,
            IAccountTokenRepository accountTokenRepository,
            IMapper mapper,
            IEmailSender emailSender,
            IAccountTokenGenerator accountTokenGenerator,
            ILinkGenerator linkGenerator,
            ILogger logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
            _accountTokenRepository = accountTokenRepository;
            _emailSender = emailSender;
            _accountTokenGenerator = accountTokenGenerator;
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        public async Task Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            _logger.Information("Начато создание нового пользователя с email: {Email}", request.Email);

            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            var defaultRole = await _roleRepository.GetRoleByNameAsync("User", cancellationToken);

            var existingUser = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existingUser != null)
            {
                _logger.Warning("Попытка регистрации с уже существующим email: {Email}", request.Email);
                throw new UserAlreadyExistsException("Пользователь с таким email уже существует.");
            }

            var userId = Guid.NewGuid();

            var user = new User
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = defaultRole.RoleId,
                Role = defaultRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Unconfirmed,
                Profile = new UserProfile(),
                RefreshToken = string.Empty,
            };

            var confirmationToken = _accountTokenGenerator.GenerateToken(TokenSize);
            var accountToken = new AccountToken
            {
                TokenId = Guid.NewGuid(),
                UserId = userId,
                Token = confirmationToken,
                Expiration = DateTime.UtcNow.AddDays(1),
                TokenType = TokenType.Confirmation
            };

            await _accountTokenRepository.AddAsync(accountToken, cancellationToken);

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.Information("Пользователь {UserId} успешно создан и сохранён в базе данных", userId);

            var confirmationLink = _linkGenerator.GenerateConfirmationLink(user.UserId, confirmationToken);

            BackgroundJob.Enqueue(() =>
                _emailSender.SendEmailAsync(user.Email,
                    "Подтверждение учётной записи",
                    $"Для подтверждения учётной записи нажмите <a href='{confirmationLink}'>здесь</a>.", cancellationToken));

            _logger.Information("Письмо с подтверждением аккаунта отправлено на email: {Email}", request.Email);
        }
    }
}
