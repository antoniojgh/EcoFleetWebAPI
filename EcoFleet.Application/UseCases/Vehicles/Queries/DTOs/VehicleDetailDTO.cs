using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.UseCases.Vehicles.Queries.DTOs
{
    public record VehicleDetailDTO(
        Guid Id,
        string LicensePlate,
        VehicleStatus Status,
        double Latitude,
        double Longitude,
        Guid? CurrentDriverId
    )
    {

        public static VehicleDetailDTO FromEntity(Vehicle vehicle) =>
            new(
                vehicle.Id.Value,
                vehicle.Plate.Value,
                vehicle.Status,
                vehicle.CurrentLocation.Latitude,
                vehicle.CurrentLocation.Longitude,
                vehicle.CurrentDriverId?.Value
            );
    }
}
