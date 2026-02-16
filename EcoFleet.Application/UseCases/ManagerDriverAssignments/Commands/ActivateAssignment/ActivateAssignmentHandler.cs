using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.ActivateAssignment
{
    public class ActivateAssignmentHandler : IRequestHandler<ActivateAssignmentCommand>
    {
        private readonly IRepositoryManagerDriverAssignment _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateAssignmentHandler(IRepositoryManagerDriverAssignment repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ActivateAssignmentCommand request, CancellationToken cancellationToken)
        {
            var assignmentId = new ManagerDriverAssignmentId(request.Id);
            var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken);

            if (assignment is null)
                throw new NotFoundException(nameof(ManagerDriverAssignment), request.Id);

            assignment.Activate();

            await _repository.Update(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
