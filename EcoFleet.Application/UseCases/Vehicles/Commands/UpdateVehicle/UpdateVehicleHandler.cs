using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleHandler : IRequestHandler<UpdateVehicleCommand>
    {
        private readonly IRepositoryVehicle _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVehicleHandler(IRepositoryVehicle repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicleId = new VehicleId(request.Id);
            var vehicle = await _repository.GetByIdAsync(vehicleId, cancellationToken);

            if (vehicle is null)
            {
                throw new NotFoundException(nameof(Vehicle), request.Id);
            }

            var plate = LicensePlate.Create(request.LicensePlate);
            var location = Geolocation.Create(request.Latitude, request.Longitude);

            vehicle.UpdatePlate(plate);
            vehicle.UpdateTelemetry(location);

            if (request.CurrentDriverId.HasValue)
            {
                var driverId = new DriverId(request.CurrentDriverId.Value);
                if (vehicle.CurrentDriverId != driverId)
                {
                    vehicle.AssignDriver(driverId);
                }
            }
            else
            {
                if (vehicle.CurrentDriverId is not null)
                {
                    vehicle.UnassignDriver();
                }
            }

            await _repository.Update(vehicle, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
