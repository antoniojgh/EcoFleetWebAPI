using FluentValidation;

namespace EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver
{
    public class UpdateDriverValidator : AbstractValidator<UpdateDriverCommand>
    {
        public UpdateDriverValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Driver Id is required.");

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
