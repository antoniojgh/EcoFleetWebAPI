using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverReinstated
{
    public record DriverReinstatedEventDTO(
        Guid Id,
        string FirstName,
        string LastName,
        string License,
        string Email,
        string? PhoneNumber,
        DateTime? DateOfBirth
    )
    {
        public static DriverReinstatedEventDTO FromEntity(Driver driver) =>
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
