using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomProductConfiguration : IEntityTypeConfiguration<CustomProduct>
    {
        public void Configure(EntityTypeBuilder<CustomProduct> builder)
        {
            builder.ToTable("CustomProducts");

            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.CustomDesignImageUrl)
                .HasMaxLength(500);

            builder.Property(cp => cp.AIRenderedPreviewUrl)
                .HasMaxLength(500);

            builder.Property(cp => cp.Notes)
                .HasMaxLength(1000);

            builder.Property(cp => cp.EstimatedPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            builder.Property(cp => cp.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(CustomProductStatus.Draft);

            builder.Property(cp => cp.AdminNotes)
                .HasMaxLength(1000);

            builder.Property(cp => cp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(cp => cp.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(cp => cp.User)
                .WithMany(u => u.CustomProducts)
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(cp => cp.Product)
                .WithMany(p => p.CustomProducts)
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(cp => cp.CustomColors)
                .WithOne(cpc => cpc.CustomProduct)
                .HasForeignKey(cpc => cpc.CustomProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(cp => cp.CartItems)
                .WithOne(ci => ci.CustomProduct)
                .HasForeignKey(ci => ci.CustomProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(cp => cp.OrderItems)
                .WithOne(oi => oi.CustomProduct)
                .HasForeignKey(oi => oi.CustomProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(cp => cp.UserId);

            builder.HasIndex(cp => cp.ProductId);

            builder.HasIndex(cp => cp.Status);

            builder.HasIndex(cp => cp.CreatedAt);
        }
    }
}
