using FluentValidation;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment
{
    public class CreateAssignmentValidator : AbstractValidator<CreateAssignmentCommand>
    {
        public CreateAssignmentValidator()
        {
            RuleFor(x => x.ManagerId)
                .NotEmpty().WithMessage("Manager Id is required.");

            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("Driver Id is required.");
        }
    }
}
