using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Core.Entities;

namespace RentIt.Bookings.Infrastructure.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.SenderId)
                   .IsRequired();

            builder.Property(m => m.ReceiverId)
                   .IsRequired();

            builder.Property(m => m.Content)
                   .IsRequired()
                   .HasMaxLength(1000); 

            builder.Property(m => m.Timestamp)
                   .IsRequired();
        }
    }
}