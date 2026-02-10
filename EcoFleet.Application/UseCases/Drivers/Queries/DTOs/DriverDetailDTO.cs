using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.UseCases.Drivers.Queries.DTOs
{
    public record DriverDetailDTO(
        Guid Id,
        string FirstName,
        string LastName,
        string License,
        DriverStatus Status,
        Guid? CurrentVehicleId
    )
    {
        public static DriverDetailDTO FromEntity(Driver driver) =>
            new(
                driver.Id.Value,
                driver.Name.FirstName,
                driver.Name.LastName,
                driver.License.Value,
                driver.Status,
                driver.CurrentVehicleId?.Value
            );
    }
}
