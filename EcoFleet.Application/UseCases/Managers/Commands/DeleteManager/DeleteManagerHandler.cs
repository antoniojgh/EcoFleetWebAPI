using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.DeleteManager
{
    public class DeleteManagerHandler : IRequestHandler<DeleteManagerCommand>
    {
        private readonly IRepositoryManager _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteManagerHandler(IRepositoryManager repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
        {
            var managerId = new ManagerId(request.Id);
            var manager = await _repository.GetByIdAsync(managerId, cancellationToken);

            if (manager is null)
                throw new NotFoundException(nameof(Manager), request.Id);

            await _repository.Delete(manager, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
