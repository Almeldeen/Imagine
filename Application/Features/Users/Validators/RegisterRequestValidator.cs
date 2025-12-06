using Application.Common.Validators;
using Application.Features.Users.DTOs;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class RegisterRequestValidator : BaseValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            ValidateEmail(x => x.Email);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters");

            RuleFor(x => x.FullName)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.FullName))
                .WithMessage("Full name must not exceed 200 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");
        }
    }
}
