using Core.Enums;

namespace Core.Entities
{
    public class UserColorSuggestion : BaseEntity
    {
        public string? UserId { get; set; }
        public int ProductId { get; set; }
        public string SuggestedColorName { get; set; } = null!;
        public string? SuggestedColorHex { get; set; }
        public string? SuggestedImageUrl { get; set; }
        public string? UserNotes { get; set; }
        public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;
        public string? AdminResponse { get; set; }

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public Product Product { get; set; } = null!;
    }
}
