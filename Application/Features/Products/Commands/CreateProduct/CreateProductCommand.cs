using Application.Common.Models;
using Application.Features.Products.DTOs;
using MediatR;
using System.Collections.Generic;
using System.IO;

namespace Application.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<BaseResponse<int>>
    {
        // Product name (required, validated by FluentValidation)
        public string Name { get; set; } = string.Empty;

        // Optional description
        public string? Description { get; set; }

        // Price (> 0)
        public decimal Price { get; set; }

        // Activation flag
        public bool IsActive { get; set; } = true;

        // Featured flag for highlighting products in UI
        public bool IsFeatured { get; set; } = false;

        // Popular flag for trending/popular sections
        public bool IsPopular { get; set; } = false;

        // Latest flag for latest drops sections
        public bool IsLatest { get; set; } = false;

        // Whether this product can be customized in the AI studio
        public bool AllowAiCustomization { get; set; } = false;

        // Required relationship: Product requires CategoryId in domain
        public int CategoryId { get; set; }

        public string? AvailableSizes { get; set; }

        // Image upload carried as a stream to keep Application independent of ASP.NET IFormFile
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }

        // Nested color structure (including images metadata) for single-request create
        public List<CreateProductColorDto> Colors { get; set; } = new();

        // All additional image file streams keyed by FileKey coming from DTO
        public Dictionary<string, Stream> ImageStreams { get; set; } = new();
        public Dictionary<string, string> ImageFileNames { get; set; } = new();
    }
}
