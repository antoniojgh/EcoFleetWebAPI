using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.DeactivateAssignment
{
    public record DeactivateAssignmentCommand(Guid Id) : IRequest;
}
