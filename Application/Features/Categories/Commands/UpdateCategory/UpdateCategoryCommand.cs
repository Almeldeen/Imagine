using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<BaseResponse<bool>>
    {
    }
}
