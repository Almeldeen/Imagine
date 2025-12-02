using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.ProductImages.DeleteProductImage
{
    public class DeleteProductImageCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
