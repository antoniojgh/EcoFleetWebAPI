using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PaginatedDTO<OrderDetailDTO>>
    {
        private readonly IRepositoryOrder _repository;

        public GetAllOrdersHandler(IRepositoryOrder repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDTO<OrderDetailDTO>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var ordersFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

            var ordersFilteredDTO = ordersFiltered.Select(OrderDetailDTO.FromEntity);

            var paginatedResult = new PaginatedDTO<OrderDetailDTO>
            {
                Items = ordersFilteredDTO,
                TotalCount = ordersFilteredDTO.Count()
            };

            return paginatedResult;
        }
    }
}
