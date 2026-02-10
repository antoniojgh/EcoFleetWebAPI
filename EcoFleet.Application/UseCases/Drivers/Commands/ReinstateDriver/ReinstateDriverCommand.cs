using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.ReinstateDriver
{
    public record ReinstateDriverCommand(Guid Id) : IRequest;
}
