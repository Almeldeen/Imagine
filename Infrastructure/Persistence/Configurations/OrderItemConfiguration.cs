using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(oi => oi.ColorName)
                .HasMaxLength(100);

            builder.Property(oi => oi.ProductImageUrl)
                .HasMaxLength(500);

            builder.Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(oi => oi.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(oi => oi.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(oi => oi.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(oi => oi.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.ProductColor)
                .WithMany(pc => pc.OrderItems)
                .HasForeignKey(oi => oi.ProductColorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(oi => oi.CustomProduct)
                .WithMany(cp => cp.OrderItems)
                .HasForeignKey(oi => oi.CustomProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(oi => oi.OrderId);

            builder.HasIndex(oi => oi.ProductColorId);

            builder.HasIndex(oi => oi.CustomProductId);
        }
    }
}
