using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RentIt.Users.Core.Entities;

namespace RentIt.Users.Infrastructure.Configurations
{
    public class AccountTokenConfiguration : IEntityTypeConfiguration<AccountToken>
    {
        public void Configure(EntityTypeBuilder<AccountToken> builder)
        {
            builder.HasKey(at => at.TokenId);

            builder.Property(at => at.Token)
                .IsRequired()
                .HasMaxLength(200); 

            builder.Property(at => at.UserId)
                .IsRequired();

            builder.Property(at => at.Expiration)
                .IsRequired();

            builder.Property(at => at.TokenType)
                .IsRequired();

            builder.HasIndex(at => at.UserId);
            builder.HasIndex(at => at.Expiration);
        }
    }
}
