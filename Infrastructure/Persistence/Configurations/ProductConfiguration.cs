using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.BasePrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(p => p.MainImageUrl)
                .HasMaxLength(500);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Colors)
                .WithOne(pc => pc.Product)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ColorSuggestions)
                .WithOne(cs => cs.Product)
                .HasForeignKey(cs => cs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.CustomProducts)
                .WithOne(cp => cp.Product)
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.WishlistItems)
                .WithOne(wi => wi.Product)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.CategoryId);

            builder.HasIndex(p => p.Name);

            builder.HasIndex(p => p.IsActive);

            builder.HasIndex(p => p.IsFeatured);

            builder.HasIndex(p => new { p.CategoryId, p.IsActive });
        }
    }
}
