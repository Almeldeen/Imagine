using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(pi => pi.AltText)
                .HasMaxLength(200);

            builder.Property(pi => pi.IsMain)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pi => pi.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(pi => pi.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(pi => pi.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(pi => pi.ProductColor)
                .WithMany(pc => pc.Images)
                .HasForeignKey(pi => pi.ProductColorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pi => pi.ProductColorId);

            builder.HasIndex(pi => new { pi.ProductColorId, pi.IsMain });

            builder.HasIndex(pi => new { pi.ProductColorId, pi.DisplayOrder });
        }
    }
}
