using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver
{
    public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand>
    {
        private readonly IRepositoryDriver _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDriverHandler(IRepositoryDriver repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), request.Id);
            }

            var name = FullName.Create(request.FirstName, request.LastName);
            var license = DriverLicense.Create(request.License);

            driver.UpdateName(name);
            driver.UpdateLicense(license);

            if (request.CurrentVehicleId.HasValue)
            {
                var vehicleId = new VehicleId(request.CurrentVehicleId.Value);
                if (driver.CurrentVehicleId != vehicleId)
                {
                    driver.AssignVehicle(vehicleId);
                }
            }
            else
            {
                if (driver.CurrentVehicleId is not null)
                {
                    driver.UnassignVehicle();
                }
            }

            await _repository.Update(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
