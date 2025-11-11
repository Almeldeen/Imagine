using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductColorConfiguration : IEntityTypeConfiguration<ProductColor>
    {
        public void Configure(EntityTypeBuilder<ProductColor> builder)
        {
            builder.ToTable("ProductColors");

            builder.HasKey(pc => pc.Id);

            builder.Property(pc => pc.ColorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pc => pc.ColorHex)
                .HasMaxLength(10);

            builder.Property(pc => pc.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pc => pc.AdditionalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(pc => pc.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(pc => pc.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pc => pc.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(pc => pc.Product)
                .WithMany(p => p.Colors)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pc => pc.Images)
                .WithOne(pi => pi.ProductColor)
                .HasForeignKey(pi => pi.ProductColorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pc => pc.CartItems)
                .WithOne(ci => ci.ProductColor)
                .HasForeignKey(ci => ci.ProductColorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(pc => pc.OrderItems)
                .WithOne(oi => oi.ProductColor)
                .HasForeignKey(oi => oi.ProductColorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(pc => pc.WishlistItems)
                .WithOne(wi => wi.ProductColor)
                .HasForeignKey(wi => wi.ProductColorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pc => pc.ProductId);

            builder.HasIndex(pc => new { pc.ProductId, pc.ColorName })
                .IsUnique();

            builder.HasIndex(pc => pc.IsAvailable);

            builder.HasIndex(pc => pc.Stock);
        }
    }
}
