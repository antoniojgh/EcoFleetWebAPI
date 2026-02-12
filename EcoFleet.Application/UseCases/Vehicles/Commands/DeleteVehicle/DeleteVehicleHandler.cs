using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Commands.DeleteVehicle
{
    public class DeleteVehicleHandler : IRequestHandler<DeleteVehicleCommand>
    {
        private readonly IRepositoryVehicle _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVehicleHandler(IRepositoryVehicle repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicleId = new VehicleId(request.Id);
            var vehicle = await _repository.GetByIdAsync(vehicleId, cancellationToken);

            if (vehicle is null)
            {
                throw new NotFoundException(nameof(Vehicle), request.Id);
            }

            if (vehicle.Status == VehicleStatus.Active)
            {
                throw new BusinessRuleException("Cannot delete a vehicle that is currently active. Unassign the driver first.");
            }

            await _repository.Delete(vehicle, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
