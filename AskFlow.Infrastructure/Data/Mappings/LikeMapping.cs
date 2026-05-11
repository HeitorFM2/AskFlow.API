using AskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AskFlow.Infrastructure.Data.Mappings
{
    public class LikeMapping : IEntityTypeConfiguration<Like>
    {
        public void Configure(EntityTypeBuilder<Like> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.CreatedAt).IsRequired();

            builder.HasIndex(l => new { l.PostId, l.UserId })
                .IsUnique()
                .HasFilter("[PostId] IS NOT NULL");
        }
    }
}
