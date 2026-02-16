using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Queries.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDTO>
    {
        private readonly IRepositoryOrder _repository;

        public GetOrderByIdHandler(IRepositoryOrder repository)
        {
            _repository = repository;
        }

        public async Task<OrderDetailDTO> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var orderId = new OrderId(request.Id);
            var order = await _repository.GetByIdAsync(orderId, cancellationToken);

            if (order is null)
                throw new NotFoundException(nameof(Order), request.Id);

            return OrderDetailDTO.FromEntity(order);
        }
    }
}
