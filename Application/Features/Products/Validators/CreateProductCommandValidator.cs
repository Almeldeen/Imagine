using Application.Features.Products.Commands.CreateProduct;
using FluentValidation;

namespace Application.Features.Products.Validators
{
    // Validator لطلب إنشاء منتج جديد
    // يتحقق من القيم الأساسية قبل وصولها للـ Handler
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must be <= 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must be <= 2000 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        }
    }
}
