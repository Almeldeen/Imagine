using Application.Common.Validators;
using Application.Features.Users.Commands.RegisterUser;
using Application.Features.Users.DTOs;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class RegisterUserCommandValidator : BaseValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Register request is required")
                .SetValidator(new RegisterRequestValidator());
        }
    }
}
