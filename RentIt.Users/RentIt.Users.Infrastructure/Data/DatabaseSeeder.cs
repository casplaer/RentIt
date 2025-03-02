using RentIt.Users.Application.Interfaces;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;
using RentIt.Users.Infrastructure.Migrations;

namespace RentIt.Users.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly RentItDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public DatabaseSeeder(
            RentItDbContext context, 
            IUserRepository userRepository, 
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task SeedAsync(CancellationToken cancellationToken)
        {
            if (_context.Users.Any() && _context.Roles.Any())
                return;

            var adminRole = new Role { RoleName = "Admin" };
            var userRole = new Role { RoleName = "User" };
            var landlordRole = new Role { RoleName = "Landlord" };

            await _roleRepository.AddAsync(adminRole, cancellationToken);
            await _roleRepository.AddAsync(userRole, cancellationToken);
            await _roleRepository.AddAsync(landlordRole, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "Adminov",
                Email = "admin@example.com",
                NormalizedEmail = "admin@example.com".ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash("testadmin"),
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active,
                Profile = new UserProfile()
            };

            adminUser.RefreshToken = _jwtProvider.GenerateRefreshToken(adminUser);

            var regularUser = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                NormalizedEmail = "john.doe@example.com".ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash("testuser"),
                RoleId = userRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active,
                Profile = new UserProfile()
            };

            regularUser.RefreshToken = _jwtProvider.GenerateRefreshToken(regularUser);

            var landlordUser = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Michael",
                LastName = "Smith",
                Email = "michael.smith@example.com",
                NormalizedEmail = "michael.smith@example.com".ToLowerInvariant(),
                PasswordHash = _passwordHasher.Hash("testlandlord"),
                RoleId = landlordRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active,
                Profile = new UserProfile()
            };

            landlordUser.RefreshToken = _jwtProvider.GenerateRefreshToken(landlordUser);

            await _userRepository.AddAsync(adminUser, cancellationToken);
            await _userRepository.AddAsync(regularUser, cancellationToken);
            await _userRepository.AddAsync(landlordUser, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
