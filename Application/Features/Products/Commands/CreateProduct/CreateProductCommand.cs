using Application.Common.Models;
using MediatR;
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

        // Required relationship: Product requires CategoryId in domain
        public int CategoryId { get; set; }

        // Image upload carried as a stream to keep Application independent of ASP.NET IFormFile
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
