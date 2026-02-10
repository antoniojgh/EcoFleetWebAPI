using FluentValidation;

namespace EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver
{
    public class CreateDriverValidator : AbstractValidator<CreateDriverCommand>
    {
        public CreateDriverValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name is too long.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name is too long.");

            RuleFor(x => x.License)
                .NotEmpty().WithMessage("Driver license is required.")
                .MaximumLength(20).WithMessage("Driver license is too long.");
        }
    }
}
