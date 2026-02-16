using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment
{
    public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
    {
        private readonly IRepositoryManagerDriverAssignment _assignmentRepository;
        private readonly IRepositoryManager _managerRepository;
        private readonly IRepositoryDriver _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAssignmentHandler(
            IRepositoryManagerDriverAssignment assignmentRepository,
            IRepositoryManager managerRepository,
            IRepositoryDriver driverRepository,
            IUnitOfWork unitOfWork)
        {
            _assignmentRepository = assignmentRepository;
            _managerRepository = managerRepository;
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var managerId = new ManagerId(request.ManagerId);
            var manager = await _managerRepository.GetByIdAsync(managerId, cancellationToken);

            if (manager is null)
                throw new NotFoundException(nameof(Manager), request.ManagerId);

            var driverId = new DriverId(request.DriverId);
            var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
                throw new NotFoundException(nameof(Driver), request.DriverId);

            var assignment = new ManagerDriverAssignment(managerId, driverId);

            await _assignmentRepository.AddAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return assignment.Id.Value;
        }
    }
}
