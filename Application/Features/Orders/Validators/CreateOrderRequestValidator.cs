using Application.Common.Validators;
using Application.Features.Orders.DTOs;
using FluentValidation;

namespace Application.Features.Orders.Validators
{
    public class CreateOrderRequestValidator : BaseValidator<CreateOrderRequestDto>
    {
        public CreateOrderRequestValidator()
        {
            ValidateRequiredString(x => x.FullName, "Full name", 150);
            ValidateRequiredString(x => x.PhoneNumber, "Phone number", 30);
            ValidateRequiredString(x => x.Address, "Address", 500);
            ValidateRequiredString(x => x.City, "City", 100);

            ValidateOptionalString(x => x.Notes, "Notes", 1000);

            RuleFor(x => x.CartUserOrSessionId)
                .NotEmpty().WithMessage("Cart identifier is required");

            RuleFor(x => x.GrandTotal)
                .GreaterThan(0).WithMessage("Grand total must be greater than 0");
        }
    }
}

