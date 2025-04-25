using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.PaymentId)
                .IsRequired();
            
            builder.Property(p => p.BookingId)
                .IsRequired();
            
            builder.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            
            builder.Property(p => p.PaymentTime)
                .IsRequired();
     
            builder.Property(p => p.Status)
                .IsRequired();
        }
    }
}