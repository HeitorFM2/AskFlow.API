using AskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AskFlow.Infrastructure.Data.Mappings
{
    public class PostMapping : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Content)
                .IsRequired()
                .HasMaxLength(280);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.CommentCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.LikeCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.HasIndex(p => p.CreatedAt)
                .IsDescending()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Posts_CreatedAt_Active");

            builder.HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
