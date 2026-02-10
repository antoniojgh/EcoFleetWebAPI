using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver
{
    public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Guid>
    {
        private readonly IRepositoryDriver _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDriverHandler(IRepositoryDriver repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
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
                driver = new Driver(name, license, vehicleId);
            }
            else
            {
                driver = new Driver(name, license);
            }

            await _repository.AddAsync(driver);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return driver.Id.Value;
        }
    }
}
