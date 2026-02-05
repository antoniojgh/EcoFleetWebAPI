using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle
{
    // Returns the Guid of the created vehicle
    public record CreateVehicleCommand(
        string LicensePlate,
        double Latitude,
        double Longitude
    ) : IRequest<Guid>;
}
