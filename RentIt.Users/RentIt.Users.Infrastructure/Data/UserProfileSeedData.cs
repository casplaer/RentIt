using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Infrastructure.Data
{
    public static class UserProfileSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var adminUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var regularUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var landlordUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var fixedDate = new DateTime(2025, 03, 02, 0, 0, 0, DateTimeKind.Utc);

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
