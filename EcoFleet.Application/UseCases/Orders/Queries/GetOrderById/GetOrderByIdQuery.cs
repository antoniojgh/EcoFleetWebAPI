using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Queries.GetOrderById
{
    public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailDTO>;
}
