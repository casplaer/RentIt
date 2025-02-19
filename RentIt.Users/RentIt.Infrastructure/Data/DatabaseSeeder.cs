using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Core.Interfaces.Repositories;

namespace RentIt.Users.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly RentItDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public DatabaseSeeder(
            RentItDbContext context, 
            IUserRepository userRepository, 
            IRoleRepository roleRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
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
                PasswordHash = "hashedpassword123",
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active
            };

            var regularUser = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                NormalizedEmail = "john.doe@example.com".ToLowerInvariant(),
                PasswordHash = "hashedpassword123",
                RoleId = userRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active
            };

            var landlordUser = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Michael",
                LastName = "Smith",
                Email = "michael.smith@example.com",
                NormalizedEmail = "michael.smith@example.com".ToLowerInvariant(),
                PasswordHash = "hashedpassword123",
                RoleId = landlordRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active
            };

            await _userRepository.AddAsync(adminUser, cancellationToken);
            await _userRepository.AddAsync(regularUser, cancellationToken);
            await _userRepository.AddAsync(landlordUser, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
