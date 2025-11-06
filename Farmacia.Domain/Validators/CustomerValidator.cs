using Farmacia.Domain.Entities;
using FluentValidation;

namespace Farmacia.Domain.Validators;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(c => c.Phone)
            .MaximumLength(30);
    }
}
