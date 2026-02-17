using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.UpdateManager
{
    public class UpdateManagerHandler : IRequestHandler<UpdateManagerCommand>
    {
        private readonly IRepositoryManager _managerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateManagerHandler(IRepositoryManager managerRepository, IUnitOfWork unitOfWork)
        {
            _managerRepository = managerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
        {
            var managerId = new ManagerId(request.Id);
            var manager = await _managerRepository.GetByIdAsync(managerId, cancellationToken);

            if (manager is null)
                throw new NotFoundException(nameof(Manager), request.Id);

            var name = FullName.Create(request.FirstName, request.LastName);
            var email = Email.Create(request.Email);

            manager.UpdateName(name);
            manager.UpdateEmail(email);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
