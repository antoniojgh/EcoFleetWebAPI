using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.DeleteVehicle
{
    public record DeleteVehicleCommand(Guid Id) : IRequest;
}
