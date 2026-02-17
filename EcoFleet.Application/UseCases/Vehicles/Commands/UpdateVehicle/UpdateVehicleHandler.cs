using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleHandler : IRequestHandler<UpdateVehicleCommand>
    {
        private readonly IRepositoryVehicle _vehicleRepository;
        private readonly IRepositoryDriver _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVehicleHandler(IRepositoryVehicle vehicleRepository, IRepositoryDriver driverRepository, IUnitOfWork unitOfWork)
        {
            _vehicleRepository = vehicleRepository;
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicleId = new VehicleId(request.Id);
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

            if (vehicle is null)
                throw new NotFoundException(nameof(Vehicle), request.Id);

            var plate = LicensePlate.Create(request.LicensePlate);
            var location = Geolocation.Create(request.Latitude, request.Longitude);

            vehicle.UpdatePlate(plate);
            vehicle.UpdateTelemetry(location);

            if (request.CurrentDriverId.HasValue)
            {
                var newDriverId = new DriverId(request.CurrentDriverId.Value);

                // Only proceed if the driver is changing to avoid unnecessary operations
                if (vehicle.CurrentDriverId != newDriverId)
                {
                    // 1. Validate the new driver exists and is available (before any mutation)
                    var newDriver = await _driverRepository.GetByIdAsync(newDriverId, cancellationToken);

                    if (newDriver is null)
                        throw new NotFoundException(nameof(Driver), request.CurrentDriverId.Value);

                    if (newDriver.Status != DriverStatus.Available)
                        throw new BusinessRuleException($"Driver {request.CurrentDriverId.Value} is not available for assignment.");

                    // 2. Release the previous driver (if any)
                    if (vehicle.CurrentDriverId is not null)
                    {
                        var previousDriver = await _driverRepository.GetByIdAsync(vehicle.CurrentDriverId, cancellationToken);
                        
                        if (previousDriver is not null)
                            previousDriver.UnassignVehicle();
                    }

                    // 3. Sync both aggregates
                    vehicle.AssignDriver(newDriverId);
                    newDriver.AssignVehicle(vehicle.Id);
                }
            }
            else
            {
                if (vehicle.CurrentDriverId is not null)
                {
                    // Release the current driver
                    var currentDriver = await _driverRepository.GetByIdAsync(vehicle.CurrentDriverId, cancellationToken);
                    
                    if (currentDriver is not null)
                        currentDriver.UnassignVehicle();


                    vehicle.UnassignDriver();
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
