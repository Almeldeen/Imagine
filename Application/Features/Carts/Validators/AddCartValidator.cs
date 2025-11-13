using Application.Common.Validators;
using Application.Features.Carts.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Carts.Validators
{
    public class AddCartValidator: BaseValidator<AddCartDto>
    {
        public AddCartValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().When(x => string.IsNullOrEmpty(x.SessionId))
                .WithMessage("Either UserId or SessionId must be provided.");

            RuleForEach(x => x.Items).SetValidator(new AddCartItemValidator());
        }
    }

    public class AddCartItemValidator : AbstractValidator<AddCartItemDto>
    {
        public AddCartItemValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
        }
    }

}

