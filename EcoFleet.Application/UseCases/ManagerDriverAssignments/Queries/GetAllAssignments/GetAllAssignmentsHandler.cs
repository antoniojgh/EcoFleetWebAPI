using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAllAssignments
{
    public class GetAllAssignmentsHandler : IRequestHandler<GetAllAssignmentsQuery, PaginatedDTO<ManagerDriverAssignmentDetailDTO>>
    {
        private readonly IRepositoryManagerDriverAssignment _repository;

        public GetAllAssignmentsHandler(IRepositoryManagerDriverAssignment repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedDTO<ManagerDriverAssignmentDetailDTO>> Handle(GetAllAssignmentsQuery request, CancellationToken cancellationToken)
        {
            var assignmentsFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

            var assignmentsFilteredDTO = assignmentsFiltered.Select(ManagerDriverAssignmentDetailDTO.FromEntity);

            var paginatedResult = new PaginatedDTO<ManagerDriverAssignmentDetailDTO>
            {
                Items = assignmentsFilteredDTO,
                TotalCount = assignmentsFilteredDTO.Count()
            };

            return paginatedResult;
        }
    }
}
