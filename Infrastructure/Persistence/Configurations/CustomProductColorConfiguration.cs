using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CustomProductColorConfiguration : IEntityTypeConfiguration<CustomProductColor>
    {
        public void Configure(EntityTypeBuilder<CustomProductColor> builder)
        {
            builder.ToTable("CustomProductColors");

            builder.HasKey(cpc => cpc.Id);

            builder.Property(cpc => cpc.ColorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(cpc => cpc.ColorHex)
                .HasMaxLength(10);

            builder.Property(cpc => cpc.ImageUrl)
                .HasMaxLength(500);

            builder.Property(cpc => cpc.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(cpc => cpc.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(cpc => cpc.CustomProduct)
                .WithMany(cp => cp.CustomColors)
                .HasForeignKey(cpc => cpc.CustomProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(cpc => cpc.CustomProductId);
        }
    }
}
