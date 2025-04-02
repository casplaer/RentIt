using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;
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
        public DbSet<AccountToken> AccountTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new AccountTokenConfiguration());

            base.OnModelCreating(modelBuilder);

            RoleSeedData.Seed(modelBuilder);
            UserSeedData.Seed(modelBuilder);
            UserProfileSeedData.Seed(modelBuilder);
        }
    }
}