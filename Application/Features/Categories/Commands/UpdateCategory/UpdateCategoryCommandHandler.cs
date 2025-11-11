using Application.Common.Models;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, BaseResponse<bool>>
    {
        public async Task<BaseResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
