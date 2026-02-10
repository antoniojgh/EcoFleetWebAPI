using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.SuspendDriver
{
    public class SuspendDriverHandler : IRequestHandler<SuspendDriverCommand>
    {
        private readonly IRepositoryDriver _repository;
        private readonly IUnitOfWork _unitOfWork;

        public SuspendDriverHandler(IRepositoryDriver repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(SuspendDriverCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), request.Id);
            }

            driver.Suspend();

            await _repository.Update(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
