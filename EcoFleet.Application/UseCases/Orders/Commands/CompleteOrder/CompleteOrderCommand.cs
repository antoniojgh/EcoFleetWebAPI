using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.CompleteOrder
{
    public record CompleteOrderCommand(Guid Id) : IRequest;
}
