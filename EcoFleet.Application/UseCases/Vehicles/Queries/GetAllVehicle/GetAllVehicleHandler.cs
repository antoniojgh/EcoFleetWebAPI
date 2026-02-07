using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Queries.GetAllVehicle
{
    public class GetAllVehicleHandler : IRequestHandler<GetAllVehicleQuery, PaginatedDTO<VehicleDetailDTO>>
    {
        private readonly IRepositoryVehicle _repository;

        public GetAllVehicleHandler(IRepositoryVehicle repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDTO<VehicleDetailDTO>> Handle(GetAllVehicleQuery request, CancellationToken cancellationToken)
        {
            var vehiclesFiltered = await _repository.GetFilteredAsync(request, cancellationToken);
            var totalVehicles = await _repository.GetTotalNumberOfRecords(cancellationToken);

            var vehiclesFilteredDTO = vehiclesFiltered.Select(VehicleDetailDTO.FromEntity);

            var paginatedResult = new PaginatedDTO<VehicleDetailDTO>
            {
                Items = vehiclesFilteredDTO,
                TotalCount = totalVehicles
            };

            return paginatedResult;
        }
    }
}
