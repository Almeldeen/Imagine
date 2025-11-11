using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(500);

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(c => c.Name)
                .IsUnique();

            builder.HasIndex(c => c.IsActive);

            builder.HasIndex(c => c.DisplayOrder);
        }
    }
}
