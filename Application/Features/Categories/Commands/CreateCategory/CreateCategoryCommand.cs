using Application.Common.Models;
using MediatR;
using System.IO;

namespace Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<BaseResponse<int>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Image upload (used with ImageService)
        public Stream? ImageStream { get; set; }
        public string? ImageFileName { get; set; }
    }
}
