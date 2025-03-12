using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Infrastructure.Data
{
    public static class RoleSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var landlordRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = adminRoleId, RoleName = "Admin" },
                new Role { RoleId = userRoleId, RoleName = "User" },
                new Role { RoleId = landlordRoleId, RoleName = "Landlord" }
            );
        }
    }
}
