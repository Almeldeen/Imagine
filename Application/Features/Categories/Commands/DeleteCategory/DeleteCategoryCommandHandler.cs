using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
