using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver
{
    public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand>
    {
        private readonly IRepositoryDriver _driverRepository;
        private readonly IRepositoryVehicle _vehicleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDriverHandler(IRepositoryDriver driverRepository, IRepositoryVehicle vehicleRepository, IUnitOfWork unitOfWork)
        {
            _driverRepository = driverRepository;
            _vehicleRepository = vehicleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
                throw new NotFoundException(nameof(Driver), request.Id);

            var name = FullName.Create(request.FirstName, request.LastName);
            var license = DriverLicense.Create(request.License);
            var email = Email.Create(request.Email);
            var phoneNumber = request.PhoneNumber is not null ? PhoneNumber.Create(request.PhoneNumber) : null;

            driver.UpdateName(name);
            driver.UpdateLicense(license);
            driver.UpdateEmail(email);
            driver.UpdatePhoneNumber(phoneNumber);
            driver.UpdateDateOfBirth(request.DateOfBirth);

            if (request.CurrentVehicleId.HasValue)
            {
                var newVehicleId = new VehicleId(request.CurrentVehicleId.Value);

                if (driver.CurrentVehicleId != newVehicleId)
                {
                    // 1. Validate the new vehicle exists and is idle (before any mutation)
                    var newVehicle = await _vehicleRepository.GetByIdAsync(newVehicleId, cancellationToken);

                    if (newVehicle is null)
                        throw new NotFoundException(nameof(Vehicle), request.CurrentVehicleId.Value);

                    if (newVehicle.Status != VehicleStatus.Idle)
                        throw new BusinessRuleException($"Vehicle {request.CurrentVehicleId.Value} is not available for assignment.");

                    // 2. Release the previous vehicle (if any)
                    if (driver.CurrentVehicleId is not null)
                    {
                        var previousVehicle = await _vehicleRepository.GetByIdAsync(driver.CurrentVehicleId, cancellationToken);
                       
                        if (previousVehicle is not null)
                            previousVehicle.UnassignDriver();
                     
                    }

                    // 3. Sync both aggregates
                    driver.AssignVehicle(newVehicleId);
                    newVehicle.AssignDriver(driver.Id);
                }
            }
            else
            {
                if (driver.CurrentVehicleId is not null)
                {
                    // Release the current vehicle
                    var currentVehicle = await _vehicleRepository.GetByIdAsync(driver.CurrentVehicleId, cancellationToken);
                    
                    if (currentVehicle is not null)
                        currentVehicle.UnassignDriver();

                    driver.UnassignVehicle();
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
