using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Guid>
    {
        private readonly IRepositoryVehicle _vehicleRepository;
        private readonly IRepositoryDriver _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateVehicleHandler(IRepositoryVehicle vehicleRepository, IRepositoryDriver driverRepository, IUnitOfWork unitOfWork)
        {
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            // 1. Convert Primitives (DTO) to Domain Value Objects
            var plate = LicensePlate.Create(request.LicensePlate);
            var location = Geolocation.Create(request.Latitude, request.Longitude);

            Vehicle vehicle;

            if (request.CurrentDriverId is not null)
            {
                var driverId = new DriverId(request.CurrentDriverId.Value);

                // 2. Validate the driver exists and is available
                var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);

                if (driver is null)
                    throw new NotFoundException(nameof(Driver), request.CurrentDriverId.Value);

                if (driver.Status != DriverStatus.Available)
                    throw new BusinessRuleException($"Driver {request.CurrentDriverId.Value} is not available for assignment.");

                // 3. Create vehicle with driver
                vehicle = new Vehicle(plate, location, driverId);

                // 4. Sync the other aggregate: mark driver as OnDuty with this vehicle
                driver.AssignVehicle(vehicle.Id);
                await _driverRepository.Update(driver, cancellationToken);
            }
            else
            {
                vehicle = new Vehicle(plate, location);
            }

            await _vehicleRepository.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return vehicle.Id.Value;
        }
    }
}
