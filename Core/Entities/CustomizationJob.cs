using Core.Enums;

namespace Core.Entities
{
    public class CustomizationJob : BaseEntity
    {
        public string? UserId { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? SourceGarmentPath { get; set; }
        public string? DesignImageUrl { get; set; }
        public string? FinalProductImageUrl { get; set; }
        public string? GeneratedGarmentUrl { get; set; }
        public string? DeApiRequestId { get; set; }
        public string? TryOnJobId { get; set; }
        public string? TryOnStatusUrl { get; set; }
        public string? TryOnResultUrl { get; set; }
        public CustomizationJobStatus Status { get; set; } = CustomizationJobStatus.PendingGeneration;
        public string? LastError { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
