using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver
{
    public record CreateDriverCommand(
        string FirstName,
        string LastName,
        string License,
        string Email,
        string? PhoneNumber,
        DateTime? DateOfBirth,
        Guid? CurrentVehicleId
    ) : IRequest<Guid>;
}
