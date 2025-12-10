using Application.Common.Models;
using Application.Features.CustomProducts.DTOs;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.CustomProducts.Commands.SaveCustomizationAsCustomProduct
{
    public class SaveCustomizationAsCustomProductCommandHandler : IRequestHandler<SaveCustomizationAsCustomProductCommand, BaseResponse<SavedCustomProductDto>>
    {
        private readonly ICustomizationJobRepository _customizationJobRepository;
        private readonly ICustomProductRepository _customProductRepository;

        public SaveCustomizationAsCustomProductCommandHandler(
            ICustomizationJobRepository customizationJobRepository,
            ICustomProductRepository customProductRepository)
        {
            _customizationJobRepository = customizationJobRepository;
            _customProductRepository = customProductRepository;
        }

        public async Task<BaseResponse<SavedCustomProductDto>> Handle(SaveCustomizationAsCustomProductCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return BaseResponse<SavedCustomProductDto>.FailureResponse("User id is required.");
            }

            if (request.CustomizationJobId <= 0)
            {
                return BaseResponse<SavedCustomProductDto>.FailureResponse("A valid customization job id is required.");
            }

            var job = await _customizationJobRepository.GetByIdAsync(request.CustomizationJobId, cancellationToken);
            if (job == null || !string.Equals(job.UserId, request.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return BaseResponse<SavedCustomProductDto>.FailureResponse("Customization job could not be found for this user.");
            }

            if (string.IsNullOrWhiteSpace(job.GeneratedGarmentUrl) && string.IsNullOrWhiteSpace(job.TryOnResultUrl))
            {
                return BaseResponse<SavedCustomProductDto>.FailureResponse("There is no generated design to save as a custom product.");
            }

            var customProduct = new CustomProduct
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                CustomDesignImageUrl = job.GeneratedGarmentUrl,
                AIRenderedPreviewUrl = job.TryOnResultUrl ?? job.GeneratedGarmentUrl,
                Notes = request.Notes,
                EstimatedPrice = 0m,
                Status = CustomProductStatus.Draft
            };

            var created = await _customProductRepository.AddAsync(customProduct, cancellationToken);

            var dto = new SavedCustomProductDto
            {
                Id = created.Id,
                ProductId = created.ProductId,
                CustomDesignImageUrl = created.CustomDesignImageUrl,
                AIRenderedPreviewUrl = created.AIRenderedPreviewUrl,
                Notes = created.Notes,
                EstimatedPrice = created.EstimatedPrice,
                Status = created.Status
            };

            return BaseResponse<SavedCustomProductDto>.SuccessResponse(dto, "Customization saved as custom product.");
        }
    }
}
