using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Core.Entities;
using RentIt.Bookings.Infrastructure.Data.Configurations;

namespace RentIt.Bookings.Infrastructure.Data
{
    public class RentItDbContext : DbContext
    {
        public RentItDbContext(DbContextOptions<RentItDbContext> options)
            : base (options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookingConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
