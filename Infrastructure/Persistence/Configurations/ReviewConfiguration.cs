using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Title)
                .HasMaxLength(200);

            builder.Property(r => r.Comment)
                .HasMaxLength(2000);

            builder.Property(r => r.IsVerifiedPurchase)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(r => r.ProductId);

            builder.HasIndex(r => r.UserId);

            builder.HasIndex(r => r.Rating);

            builder.HasIndex(r => r.IsApproved);

            builder.HasIndex(r => new { r.ProductId, r.UserId });

            builder.HasIndex(r => r.CreatedAt);

            // Check constraint: Rating must be between 1 and 5
            builder.HasCheckConstraint(
                "CK_Review_Rating",
                "[Rating] >= 1 AND [Rating] <= 5"
            );
        }
    }
}
