using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.OrderDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.SubTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(o => o.ShippingCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(o => o.Tax)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(OrderStatus.Pending);

            // Shipping Information
            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.ShippingCountry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ShippingPhone)
                .HasMaxLength(20);

            // Payment Information
            builder.Property(o => o.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(o => o.PaymentTransactionId)
                .HasMaxLength(200);

            builder.Property(o => o.PaidAt)
                .IsRequired(false);

            // Tracking
            builder.Property(o => o.TrackingNumber)
                .HasMaxLength(100);

            builder.Property(o => o.ShippedAt)
                .IsRequired(false);

            builder.Property(o => o.DeliveredAt)
                .IsRequired(false);

            builder.Property(o => o.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.HasIndex(o => o.UserId);

            builder.HasIndex(o => o.Status);

            builder.HasIndex(o => o.OrderDate);

            builder.HasIndex(o => o.CreatedAt);

            builder.HasIndex(o => new { o.UserId, o.Status });
        }
    }
}
