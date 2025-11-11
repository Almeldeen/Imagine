using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserColorSuggestionConfiguration : IEntityTypeConfiguration<UserColorSuggestion>
    {
        public void Configure(EntityTypeBuilder<UserColorSuggestion> builder)
        {
            builder.ToTable("UserColorSuggestions");

            builder.HasKey(ucs => ucs.Id);

            builder.Property(ucs => ucs.SuggestedColorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ucs => ucs.SuggestedColorHex)
                .HasMaxLength(10);

            builder.Property(ucs => ucs.SuggestedImageUrl)
                .HasMaxLength(500);

            builder.Property(ucs => ucs.UserNotes)
                .HasMaxLength(1000);

            builder.Property(ucs => ucs.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(SuggestionStatus.Pending);

            builder.Property(ucs => ucs.AdminResponse)
                .HasMaxLength(1000);

            builder.Property(ucs => ucs.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ucs => ucs.UpdatedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(ucs => ucs.User)
                .WithMany(u => u.ColorSuggestions)
                .HasForeignKey(ucs => ucs.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ucs => ucs.Product)
                .WithMany(p => p.ColorSuggestions)
                .HasForeignKey(ucs => ucs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ucs => ucs.UserId);

            builder.HasIndex(ucs => ucs.ProductId);

            builder.HasIndex(ucs => ucs.Status);

            builder.HasIndex(ucs => ucs.CreatedAt);
        }
    }
}
