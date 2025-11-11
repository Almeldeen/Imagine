using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.ToTable("Wishlists");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("My Wishlist");

            builder.Property(w => w.IsDefault)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(w => w.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(w => w.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(w => w.Items)
                .WithOne(wi => wi.Wishlist)
                .HasForeignKey(wi => wi.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(w => w.UserId);

            builder.HasIndex(w => new { w.UserId, w.IsDefault });
        }
    }
}
