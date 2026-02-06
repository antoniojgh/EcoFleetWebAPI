using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.MarkForMaintenance
{
    public record MarkForMaintenanceCommand(Guid Id) : IRequest;
}
