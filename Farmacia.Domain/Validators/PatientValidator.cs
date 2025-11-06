using Farmacia.Domain.Entities;
using FluentValidation;

namespace Farmacia.Domain.Validators;

public class PatientValidator : AbstractValidator<Patient>
{
    public PatientValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(p => p.Phone)
            .MaximumLength(30);
    }
}
