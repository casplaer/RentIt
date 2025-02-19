using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Infrastructure.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("user_profiles");

            builder.HasKey(up => up.UserId);

            builder.Property(up => up.UserId)
                .HasColumnName("user_id");

            builder.Property(up => up.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(50);

            builder.Property(up => up.Address)
                .HasColumnName("address")
                .HasMaxLength(200);

            builder.Property(up => up.City)
                .HasColumnName("city")
                .HasMaxLength(100);

            builder.Property(up => up.Country)
                .HasColumnName("country")
                .HasMaxLength(100);

            builder.Property(up => up.CreatedAt)
                .HasColumnName("created_at");

            builder.HasOne(up => up.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(up => up.UserId);
        }
    }
}
