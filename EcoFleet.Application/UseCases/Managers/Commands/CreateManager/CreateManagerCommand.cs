using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.CreateManager
{
    public record CreateManagerCommand(
        string FirstName,
        string LastName,
        string Email
    ) : IRequest<Guid>;
}
