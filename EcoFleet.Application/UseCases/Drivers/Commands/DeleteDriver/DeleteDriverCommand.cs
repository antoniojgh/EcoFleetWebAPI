using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.DeleteDriver
{
    public record DeleteDriverCommand(Guid Id) : IRequest;
}
