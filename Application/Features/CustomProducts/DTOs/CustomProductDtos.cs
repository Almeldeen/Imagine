using Core.Enums;

namespace Application.Features.CustomProducts.DTOs
{
    public class SavedCustomProductDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? CustomDesignImageUrl { get; set; }
        public string? AIRenderedPreviewUrl { get; set; }
        public string? Notes { get; set; }
        public decimal EstimatedPrice { get; set; }
        public CustomProductStatus Status { get; set; }
    }
}
