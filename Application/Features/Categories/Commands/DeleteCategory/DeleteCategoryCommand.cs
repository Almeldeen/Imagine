using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }
    }
}
