using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<BaseResponse<bool>>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Optional new image upload
        public Stream? NewImageStream { get; set; }
        public string? NewImageFileName { get; set; }
    }
}
