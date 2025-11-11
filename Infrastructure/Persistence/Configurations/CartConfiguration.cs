using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.SessionId)
                .HasMaxLength(200);

            builder.Property(c => c.ExpiresAt)
                .IsRequired(false);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            // Ignore computed properties
            builder.Ignore(c => c.TotalAmount);
            builder.Ignore(c => c.TotalItems);

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.UserId);

            builder.HasIndex(c => c.SessionId);

            builder.HasIndex(c => c.ExpiresAt);

            builder.HasIndex(c => c.CreatedAt);
        }
    }
}
