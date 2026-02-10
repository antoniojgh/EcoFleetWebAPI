using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Commands.ReinstateDriver
{
    public class ReinstateDriverHandler : IRequestHandler<ReinstateDriverCommand>
    {
        private readonly IRepositoryDriver _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ReinstateDriverHandler(IRepositoryDriver repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ReinstateDriverCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), request.Id);
            }

            driver.Reinstate();

            await _repository.Update(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
