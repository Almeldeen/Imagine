using Application.Common.Validators;
using Application.Features.Users.DTOs;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class LoginRequestValidator : BaseValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier is required")
                .MaximumLength(200).WithMessage("Identifier must not exceed 200 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
}
