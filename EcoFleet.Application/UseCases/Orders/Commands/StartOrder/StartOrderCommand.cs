using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.StartOrder
{
    public record StartOrderCommand(Guid Id) : IRequest;
}
