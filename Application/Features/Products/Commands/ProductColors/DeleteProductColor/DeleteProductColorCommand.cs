using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.ProductColors.DeleteProductColor
{
    public class DeleteProductColorCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
