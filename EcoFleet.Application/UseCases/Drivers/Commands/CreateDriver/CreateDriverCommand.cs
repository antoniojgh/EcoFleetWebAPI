using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver
{
    public record CreateDriverCommand(
        string FirstName,
        string LastName,
        string License,
        Guid? CurrentVehicleId
    ) : IRequest<Guid>;
}
