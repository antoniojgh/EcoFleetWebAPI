using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Queries.DTOs;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Queries.GetManagerById
{
    public class GetManagerByIdHandler : IRequestHandler<GetManagerByIdQuery, ManagerDetailDTO>
    {
        private readonly IRepositoryManager _repository;

        public GetManagerByIdHandler(IRepositoryManager repository)
        {
            _repository = repository;
        }

        public async Task<ManagerDetailDTO> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
        {
            var managerId = new ManagerId(request.Id);
            var manager = await _repository.GetByIdAsync(managerId, cancellationToken);

            if (manager is null)
                throw new NotFoundException(nameof(Manager), request.Id);

            return ManagerDetailDTO.FromEntity(manager);
        }
    }
}
