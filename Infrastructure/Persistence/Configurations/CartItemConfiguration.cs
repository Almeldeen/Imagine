using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ci => ci.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(ci => ci.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(ci => ci.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ci => ci.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.ProductColor)
                .WithMany(pc => pc.CartItems)
                .HasForeignKey(ci => ci.ProductColorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ci => ci.CustomProduct)
                .WithMany(cp => cp.CartItems)
                .HasForeignKey(ci => ci.CustomProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(ci => ci.CartId);

            builder.HasIndex(ci => ci.ProductColorId);

            builder.HasIndex(ci => ci.CustomProductId);

            // Check constraint: Either ProductColorId or CustomProductId must be set
            builder.HasCheckConstraint(
                "CK_CartItem_ProductOrCustomProduct",
                "([ProductColorId] IS NOT NULL AND [CustomProductId] IS NULL) OR ([ProductColorId] IS NULL AND [CustomProductId] IS NOT NULL)"
            );
        }
    }
}
