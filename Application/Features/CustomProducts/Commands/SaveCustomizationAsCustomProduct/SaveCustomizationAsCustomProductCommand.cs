using Application.Common.Models;
using Application.Features.CustomProducts.DTOs;
using MediatR;

namespace Application.Features.CustomProducts.Commands.SaveCustomizationAsCustomProduct
{
    public class SaveCustomizationAsCustomProductCommand : IRequest<BaseResponse<SavedCustomProductDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public int CustomizationJobId { get; set; }
        public int? ProductId { get; set; }
        public string? Notes { get; set; }

        // Optional variant metadata coming from the client studio
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public string? Size { get; set; }
    }
}
