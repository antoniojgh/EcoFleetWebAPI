using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Queries.GetVehicleById
{
    public class GetVehicleByIdHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDetailDTO>
    {
        private readonly IRepositoryVehicle _repository;

        public GetVehicleByIdHandler(IRepositoryVehicle repository)
        {
            _repository = repository;
        }

        public async Task<VehicleDetailDTO> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        {
            var vehicleId = new VehicleId(request.Id);
            var vehicle = await _repository.GetByIdAsync(vehicleId, cancellationToken);

            if (vehicle is null)
            {
                throw new NotFoundException(nameof(Vehicle), request.Id);
            }

            return VehicleDetailDTO.FromEntity(vehicle);
        }
    }
}
