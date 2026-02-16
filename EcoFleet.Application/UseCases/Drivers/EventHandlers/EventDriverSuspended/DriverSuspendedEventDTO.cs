using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverSuspended
{
    public record DriverSuspendedEventDTO(
        Guid Id,
        string FirstName,
        string LastName,
        string License,
        string Email,
        string? PhoneNumber,
        DateTime? DateOfBirth
    )
    {
        public static DriverSuspendedEventDTO FromEntity(Driver driver) =>
            new(
                driver.Id.Value,
                driver.Name.FirstName,
                driver.Name.LastName,
                driver.License.Value,
                driver.Email.Value,
                driver.PhoneNumber?.Value,
                driver.DateOfBirth
            );
    }
}
