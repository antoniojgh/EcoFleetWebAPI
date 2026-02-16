using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.DeleteManager
{
    public record DeleteManagerCommand(Guid Id) : IRequest;
}
