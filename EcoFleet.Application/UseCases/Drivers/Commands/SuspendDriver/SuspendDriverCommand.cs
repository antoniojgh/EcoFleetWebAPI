using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.SuspendDriver
{
    public record SuspendDriverCommand(Guid Id) : IRequest;
}
