using MediatR;
using RentIt.Users.Application.Exceptions;
using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Application.Commands.Users.Create
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailNormalizer _emailNormalizer;

        public CreateUserCommandHandler(
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher,
            IEmailNormalizer emailNormalizer)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailNormalizer = emailNormalizer;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = _emailNormalizer.NormalizeEmail(request.Email);

            var existingUser = await _userRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
            if (existingUser != null)
                throw new UserAlreadyExistsException("Пользователь с таким email уже существует.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = request.Role.RoleId,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
            return user.UserId;
        }
    }
}
