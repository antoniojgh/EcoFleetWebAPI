using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.UpdateManager
{
    public record UpdateManagerCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Email
    ) : IRequest;
}
