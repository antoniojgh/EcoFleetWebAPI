using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver
{
    public record UpdateDriverCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string License,
        string Email,
        string? PhoneNumber,
        DateTime? DateOfBirth,
        Guid? CurrentVehicleId
    ) : IRequest;
}
