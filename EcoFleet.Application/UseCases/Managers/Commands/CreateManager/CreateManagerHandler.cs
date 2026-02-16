using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Commands.CreateManager
{
    public class CreateManagerHandler : IRequestHandler<CreateManagerCommand, Guid>
    {
        private readonly IRepositoryManager _managerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateManagerHandler(IRepositoryManager managerRepository, IUnitOfWork unitOfWork)
        {
            _managerRepository = managerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
        {
            var name = FullName.Create(request.FirstName, request.LastName);
            var email = Email.Create(request.Email);

            var manager = new Manager(name, email);

            await _managerRepository.AddAsync(manager, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return manager.Id.Value;
        }
    }
}
