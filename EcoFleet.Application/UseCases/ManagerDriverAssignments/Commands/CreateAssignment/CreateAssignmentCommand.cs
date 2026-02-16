using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment
{
    public record CreateAssignmentCommand(
        Guid ManagerId,
        Guid DriverId
    ) : IRequest<Guid>;
}
