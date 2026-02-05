using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Guid>
    {
        private readonly IVehicleRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        // We inject the interfaces, NOT the concrete EF Core class
        public CreateVehicleHandler(IVehicleRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            // We created a MediatR Pipeline Behavior for autimatically validating the command using FluentValidation
            // so we can be sure that the command's data is valid at this point.

            // 1. Convert Primitives (DTO) to Domain Value Objects
            // If the data is invalid, the Value Object constructor throws a DomainException
            var plate = LicensePlate.Create(request.LicensePlate);
            var location = Geolocation.Create(request.Latitude, request.Longitude);

            // 2. Create the Aggregate Root using the Business Constructor
            var vehicle = new Vehicle(plate, location);

            // 3. Add to Repository (Memory/Tracking)
            _repository.Add(vehicle);

            // 4. Commit Transaction (Database Write + Domain Events Dispatch)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return vehicle.Id.Value;
        }
    }
}
