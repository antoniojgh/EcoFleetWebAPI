using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle
{
    public record UpdateVehicleCommand(
        Guid Id,
        string LicensePlate,
        double Latitude,
        double Longitude,
        Guid? CurrentDriverId
    ) : IRequest;
}
