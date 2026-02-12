using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.DeleteDriver
{
    public class DeleteDriverHandler : IRequestHandler<DeleteDriverCommand>
    {
        private readonly IRepositoryDriver _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDriverHandler(IRepositoryDriver repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), request.Id);
            }

            if (driver.Status == DriverStatus.OnDuty)
            {
                throw new BusinessRuleException("Cannot delete a driver who is currently on duty. Unassign them first.");
            }

            await _repository.Delete(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
