using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Products.Commands.ProductImages.AddProductImage
{
    public class AddProductImageCommand : IRequest<BaseResponse<int>>
    {
        public int ProductColorId { get; set; }
        public string? AltText { get; set; }
        public bool? IsMain { get; set; }

        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
