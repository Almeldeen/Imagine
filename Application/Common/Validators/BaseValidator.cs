using FluentValidation;
using System.Linq.Expressions;

namespace Application.Common.Validators
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        protected void ValidateId(Expression<Func<T, int>> expression, string propertyName = "Id")
        {
            RuleFor(expression)
                .GreaterThan(0)
                .WithMessage($"{propertyName} must be greater than 0");
        }

        protected void ValidateRequiredString(Expression<Func<T, string>> expression, string propertyName, int maxLength = 200)
        {
            RuleFor(expression)
                .NotEmpty()
                .WithMessage($"{propertyName} is required")
                .MaximumLength(maxLength)
                .WithMessage($"{propertyName} must not exceed {maxLength} characters");
        }

        protected void ValidateOptionalString(Expression<Func<T, string?>> expression, string propertyName, int maxLength = 500)
        {
            RuleFor(expression)
                .MaximumLength(maxLength)
                .When(x => !string.IsNullOrEmpty(expression.Compile()(x)))
                .WithMessage($"{propertyName} must not exceed {maxLength} characters");
        }

        protected void ValidateEmail(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email is not valid")
                .MaximumLength(200)
                .WithMessage("Email must not exceed 200 characters");
        }

        protected void ValidatePrice(Expression<Func<T, decimal>> expression, string propertyName = "Price")
        {
            RuleFor(expression)
                .GreaterThanOrEqualTo(0)
                .WithMessage($"{propertyName} must be greater than or equal to 0")
                .LessThan(1000000)
                .WithMessage($"{propertyName} must be less than 1,000,000");
        }

        protected void ValidateQuantity(Expression<Func<T, int>> expression, string propertyName = "Quantity")
        {
            RuleFor(expression)
                .GreaterThan(0)
                .WithMessage($"{propertyName} must be greater than 0")
                .LessThanOrEqualTo(1000)
                .WithMessage($"{propertyName} must not exceed 1000");
        }
    }
}
