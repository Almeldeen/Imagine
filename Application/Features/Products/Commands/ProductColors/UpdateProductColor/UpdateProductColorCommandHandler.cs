using Application.Common.Exceptions;
using Application.Common.Models;
using Core.Interfaces;
using MediatR;

namespace Application.Features.Products.Commands.ProductColors.UpdateProductColor
{
    public class UpdateProductColorCommandHandler : IRequestHandler<UpdateProductColorCommand, BaseResponse<bool>>
    {
        private readonly IProductColorRepository _productColorRepository;

        public UpdateProductColorCommandHandler(IProductColorRepository productColorRepository)
        {
            _productColorRepository = productColorRepository;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateProductColorCommand request, CancellationToken cancellationToken)
        {
            var color = await _productColorRepository.GetByIdAsync(request.Id, cancellationToken);
            if (color == null)
            {
                throw new NotFoundException("ProductColor", request.Id);
            }

            color.ColorName = request.ColorName;
            color.ColorHex = request.ColorHex;
            color.Stock = request.Stock;
            color.AdditionalPrice = request.AdditionalPrice;
            color.IsAvailable = request.IsAvailable;
            color.UpdatedAt = DateTime.UtcNow;

            await _productColorRepository.UpdateAsync(color, cancellationToken);

            return BaseResponse<bool>.SuccessResponse(true, "Product color updated successfully");
        }
    }
}
