using AskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AskFlow.Infrastructure.Data.Mappings
{
    public class RefreshTokenMapping : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.TokenHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(r => r.TokenHash).IsUnique();

            builder.Property(r => r.ExpiresAt).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
