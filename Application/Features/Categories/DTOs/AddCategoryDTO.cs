namespace Application.Features.Categories.DTOs
{
    // Simple DTO used when creating a new category
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }

    // Simple DTO used when updating an existing category (Id comes from route in most cases)
    public class UpdateCategoryDto : CreateCategoryDto
    {
    }
}
