using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Queries.GetAllDrivers
{
    public class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, PaginatedDTO<DriverDetailDTO>>
    {
        private readonly IRepositoryDriver _repository;

        public GetAllDriversHandler(IRepositoryDriver repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDTO<DriverDetailDTO>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var driversFiltered = await _repository.GetFilteredAsync(request, cancellationToken);
            var totalDrivers = await _repository.GetTotalNumberOfRecords(cancellationToken);

            var driversFilteredDTO = driversFiltered.Select(DriverDetailDTO.FromEntity);

            var paginatedResult = new PaginatedDTO<DriverDetailDTO>
            {
                Items = driversFilteredDTO,
                TotalCount = totalDrivers
            };

            return paginatedResult;
        }
    }
}
