using Application.Common.Validators;
using Application.Features.Users.Queries.LoginUser;
using Application.Features.Users.Validators;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class LoginUserQueryValidator : BaseValidator<LoginUserQuery>
    {
        public LoginUserQueryValidator()
        {
            RuleFor(x => x.Request)
                .NotNull().WithMessage("Login request is required")
                .SetValidator(new LoginRequestValidator());
        }
    }
}
