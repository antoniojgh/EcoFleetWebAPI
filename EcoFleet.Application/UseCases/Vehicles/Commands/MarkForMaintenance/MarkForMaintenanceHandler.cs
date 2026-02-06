using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.MarkForMaintenance
{
    public class MarkForMaintenanceHandler : IRequestHandler<MarkForMaintenanceCommand>
    {
        private readonly IRepositoryVehicle _repository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkForMaintenanceHandler(IRepositoryVehicle repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(MarkForMaintenanceCommand request, CancellationToken cancellationToken)
        {
            var vehicleId = new VehicleId(request.Id);
            var vehicle = await _repository.GetByIdAsync(vehicleId, cancellationToken);

            if (vehicle is null)
            {
                throw new NotFoundException(nameof(Vehicle), request.Id);
            }

            vehicle.MarkForMaintenance();

            await _repository.Update(vehicle, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
