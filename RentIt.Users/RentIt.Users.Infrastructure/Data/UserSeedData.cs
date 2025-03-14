using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
using RentIt.Users.Core.Enums;

namespace RentIt.Users.Infrastructure.Data
{
    public static class UserSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
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
                    RoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
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
                    RoleId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
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
                    RoleId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    CreatedAt = fixedDate,
                    UpdatedAt = fixedDate,
                    Status = UserStatus.Active,
                    RefreshToken = "TEST_REFRESH_TOKEN_LANDLORD",
                    RefreshTokenExpiryTime = fixedDate.AddDays(7)
                }
            );
        }
    }
}
