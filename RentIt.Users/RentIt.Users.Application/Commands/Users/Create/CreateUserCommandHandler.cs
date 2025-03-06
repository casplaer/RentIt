using AutoMapper;
using FluentValidation;
using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailNormalizer _emailNormalizer;
        private readonly IValidator<CreateUserCommand> _validator;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer,
            IMapper mapper,
            IValidator<CreateUserCommand> validator)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
            _validator = validator;
        }

        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);
            var defaultRole = await _roleRepository.GetRoleByNameAsync("User", cancellationToken);

            var existingUser = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = defaultRole.RoleId,
                Role = defaultRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active,
                Profile = new UserProfile(),
                RefreshToken = string.Empty,
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
