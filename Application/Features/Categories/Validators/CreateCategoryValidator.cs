using Application.Common.Validators;
using Application.Features.Categories.Commands.CreateCategory;
using FluentValidation;

namespace Application.Features.Categories.Validators
{
    public class CreateCategoryValidator : BaseValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Category name is required")
                .MaximumLength(100)
                .WithMessage("Category name must not exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.ImagePath)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.ImagePath))
                .WithMessage("Image path must not exceed 500 characters");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Display order must be greater than or equal to 0");
        }
    }
}
