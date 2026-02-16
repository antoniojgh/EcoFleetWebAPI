using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Queries.GetAllManagers
{
    public class GetAllManagersHandler : IRequestHandler<GetAllManagersQuery, PaginatedDTO<ManagerDetailDTO>>
    {
        private readonly IRepositoryManager _repository;

        public GetAllManagersHandler(IRepositoryManager repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDTO<ManagerDetailDTO>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
        {
            var managersFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

            var managersFilteredDTO = managersFiltered.Select(ManagerDetailDTO.FromEntity);

            var paginatedResult = new PaginatedDTO<ManagerDetailDTO>
            {
                Items = managersFilteredDTO,
                TotalCount = managersFilteredDTO.Count()
            };

            return paginatedResult;
        }
    }
}
