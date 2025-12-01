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

        // Optional new image
        public Stream? NewImageStream { get; set; }
        public string? NewImageFileName { get; set; }
    }
}
