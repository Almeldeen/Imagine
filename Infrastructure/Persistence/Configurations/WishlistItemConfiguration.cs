using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("WishlistItems");

            builder.HasKey(wi => wi.Id);

            builder.Property(wi => wi.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(wi => wi.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(wi => wi.Wishlist)
                .WithMany(w => w.Items)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wi => wi.ProductColor)
                .WithMany(pc => pc.WishlistItems)
                .HasForeignKey(wi => wi.ProductColorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(wi => wi.WishlistId);

            builder.HasIndex(wi => wi.ProductId);

            builder.HasIndex(wi => wi.ProductColorId);

            builder.HasIndex(wi => new { wi.WishlistId, wi.ProductId, wi.ProductColorId })
                .IsUnique();
        }
    }
}
