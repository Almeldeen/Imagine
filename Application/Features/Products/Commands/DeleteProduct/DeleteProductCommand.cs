using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<BaseResponse<bool>>
    {
        // Target product Id to delete
        public int Id { get; set; }
    }
}
