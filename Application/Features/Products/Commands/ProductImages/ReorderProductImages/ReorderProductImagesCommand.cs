using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Products.Commands.ProductImages.ReorderProductImages
{
    public class ReorderProductImagesCommand : IRequest<BaseResponse<bool>>
    {
        public int ProductColorId { get; set; }
        public List<int> ImageIdsInOrder { get; set; } = new();
    }
}
