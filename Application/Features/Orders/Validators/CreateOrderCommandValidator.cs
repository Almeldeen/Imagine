using Application.Common.Validators;
using Application.Features.Orders.Commands.CreateOrder;
using FluentValidation;

namespace Application.Features.Orders.Validators
{
    public class CreateOrderCommandValidator : BaseValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            ValidateRequiredString(x => x.UserId, "UserId", 450);

            RuleFor(x => x.Request)
                .NotNull().WithMessage("Order details are required")
                .SetValidator(new CreateOrderRequestValidator());
        }
    }
}

