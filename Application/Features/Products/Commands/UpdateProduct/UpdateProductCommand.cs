using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<BaseResponse<bool>>
    {
        // Target product Id
        public int Id { get; set; }

        // New values
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public int CategoryId { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPopular { get; set; }
        public bool IsLatest { get; set; }
        public bool AllowAiCustomization { get; set; }
        public string? AvailableSizes { get; set; }

        // Optional new image
        public Stream? NewImageStream { get; set; }
        public string? NewImageFileName { get; set; }
    }
}
