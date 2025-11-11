namespace Core.Entities
{
    public class Review : BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = null!;
        public int Rating { get; set; } // 1-5 stars
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
