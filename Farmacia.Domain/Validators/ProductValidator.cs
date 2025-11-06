using Farmacia.Domain.Entities;
using FluentValidation;

namespace Farmacia.Domain.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(p => p.Cost)
            .GreaterThanOrEqualTo(0);

        RuleFor(p => p.StockMinimum)
            .GreaterThanOrEqualTo(0);

        RuleFor(p => p.TaxRate)
            .InclusiveBetween(0, 1);
    }
}
