using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;
using RentIt.Users.Infrastructure.Configurations;

namespace RentIt.Users.Infrastructure.Data
{
    public class RentItDbContext : DbContext
    {
        public RentItDbContext(DbContextOptions<RentItDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            base.OnModelCreating(modelBuilder);

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var landlordRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = adminRoleId, RoleName = "Admin" },
                new Role { RoleId = userRoleId, RoleName = "User" },
                new Role { RoleId = landlordRoleId, RoleName = "Landlord" }
            );

            var adminUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var regularUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var landlordUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            var fixedDate = new DateTime(2025, 03, 02, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = adminUserId,
                    FirstName = "Admin",
                    LastName = "Adminov",
                    Email = "admin@example.com",
                    NormalizedEmail = "admin@example.com".ToLowerInvariant(),
                    PasswordHash = "HASHED_testadmin",
                    RoleId = adminRoleId,
                    CreatedAt = fixedDate,
                    UpdatedAt = fixedDate,
                    Status = UserStatus.Active,
                    RefreshToken = "TEST_REFRESH_TOKEN_ADMIN",
                    RefreshTokenExpiryTime = fixedDate.AddDays(7)
                },
                new User
                {
                    UserId = regularUserId,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    NormalizedEmail = "john.doe@example.com".ToLowerInvariant(),
                    PasswordHash = "HASHED_testuser",
                    RoleId = userRoleId,
                    CreatedAt = fixedDate,
                    UpdatedAt = fixedDate,
                    Status = UserStatus.Active,
                    RefreshToken = "TEST_REFRESH_TOKEN_USER",
                    RefreshTokenExpiryTime = fixedDate.AddDays(7)
                },
                new User
                {
                    UserId = landlordUserId,
                    FirstName = "Michael",
                    LastName = "Smith",
                    Email = "michael.smith@example.com",
                    NormalizedEmail = "michael.smith@example.com".ToLowerInvariant(),
                    PasswordHash = "HASHED_testlandlord",
                    RoleId = landlordRoleId,
                    CreatedAt = fixedDate,
                    UpdatedAt = fixedDate,
                    Status = UserStatus.Active,
                    RefreshToken = "TEST_REFRESH_TOKEN_LANDLORD",
                    RefreshTokenExpiryTime = fixedDate.AddDays(7)
                }
            );

            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile
                {
                    UserId = adminUserId,
                    PhoneNumber = "111-111-1111",
                    Address = "Admin Address",
                    City = "Admin City",
                    Country = "Admin Country",
                    CreatedAt = fixedDate
                },
                new UserProfile
                {
                    UserId = regularUserId,
                    PhoneNumber = "222-222-2222",
                    Address = "Doe Address",
                    City = "Doe City",
                    Country = "Doe Country",
                    CreatedAt = fixedDate
                },
                new UserProfile
                {
                    UserId = landlordUserId,
                    PhoneNumber = "333-333-3333",
                    Address = "Smith Address",
                    City = "Smith City",
                    Country = "Smith Country",
                    CreatedAt = fixedDate
                }
            );
        }
    }
}
