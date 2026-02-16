using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.ActivateAssignment
{
    public record ActivateAssignmentCommand(Guid Id) : IRequest;
}
