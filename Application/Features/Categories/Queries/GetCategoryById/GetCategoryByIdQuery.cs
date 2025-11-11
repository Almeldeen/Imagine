using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQuery : IRequest<BaseResponse<object>>
    {
    }
}
