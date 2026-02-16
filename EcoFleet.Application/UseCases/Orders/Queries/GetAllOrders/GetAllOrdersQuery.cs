using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Queries.GetAllOrders
{
    public record GetAllOrdersQuery : FilterOrderDTO, IRequest<PaginatedDTO<OrderDetailDTO>>;
}
