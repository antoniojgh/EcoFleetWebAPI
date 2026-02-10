using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver
{
    public record UpdateDriverCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string License,
        Guid? CurrentVehicleId
    ) : IRequest;
}
