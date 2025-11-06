using Farmacia.Domain.Entities;
using FluentValidation;

namespace Farmacia.Domain.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(u => u.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(u => u.PasswordHash)
            .NotEmpty();
    }
}
