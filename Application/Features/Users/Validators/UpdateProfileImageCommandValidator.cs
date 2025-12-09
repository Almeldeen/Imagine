using Application.Common.Validators;
using Application.Features.Users.Commands.UpdateProfileImage;
using FluentValidation;

namespace Application.Features.Users.Validators
{
    public class UpdateProfileImageCommandValidator : BaseValidator<UpdateProfileImageCommand>
    {
        public UpdateProfileImageCommandValidator()
        {
            ValidateRequiredString(x => x.UserId, "UserId");

            RuleFor(x => x.ImageStream)
                .NotNull()
                .WithMessage("Image stream is required.");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("File name must not exceed 255 characters.");
        }
    }
}
