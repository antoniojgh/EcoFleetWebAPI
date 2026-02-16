using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.CancelOrder
{
    public record CancelOrderCommand(Guid Id) : IRequest;
}
