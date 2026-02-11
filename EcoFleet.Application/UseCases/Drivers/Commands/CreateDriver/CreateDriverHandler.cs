using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver
{
    public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Guid>
    {
        private readonly IRepositoryDriver _driverRepository;
        private readonly IRepositoryVehicle _vehicleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDriverHandler(IRepositoryDriver driverRepository, IRepositoryVehicle vehicleRepository, IUnitOfWork unitOfWork)
        {
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var name = FullName.Create(request.FirstName, request.LastName);
            var license = DriverLicense.Create(request.License);

            Driver driver;

            if (request.CurrentVehicleId is not null)
            {
                var vehicleId = new VehicleId(request.CurrentVehicleId.Value);

                // Validate the vehicle exists and is idle
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);

                if (vehicle is null)
                    throw new NotFoundException(nameof(Vehicle), request.CurrentVehicleId.Value);

                if (vehicle.Status != VehicleStatus.Idle)
                    throw new BusinessRuleException($"Vehicle {request.CurrentVehicleId.Value} is not available for assignment.");

                // Create driver assigned to vehicle
                driver = new Driver(name, license, vehicleId);

                // Sync the other aggregate: mark vehicle as Active with this driver
                vehicle.AssignDriver(driver.Id);
                await _vehicleRepository.Update(vehicle, cancellationToken);
            }
            else
            {
                driver = new Driver(name, license);
            }

            await _driverRepository.AddAsync(driver);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return driver.Id.Value;
        }
    }
}
