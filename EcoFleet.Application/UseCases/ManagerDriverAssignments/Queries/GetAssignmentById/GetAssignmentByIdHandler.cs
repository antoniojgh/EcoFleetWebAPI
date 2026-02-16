using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAssignmentById
{
    public class GetAssignmentByIdHandler : IRequestHandler<GetAssignmentByIdQuery, ManagerDriverAssignmentDetailDTO>
    {
        private readonly IRepositoryManagerDriverAssignment _repository;

        public GetAssignmentByIdHandler(IRepositoryManagerDriverAssignment repository)
        {
            _repository = repository;
        }

        public async Task<ManagerDriverAssignmentDetailDTO> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
        {
            var assignmentId = new ManagerDriverAssignmentId(request.Id);
            var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken);

            if (assignment is null)
                throw new NotFoundException(nameof(ManagerDriverAssignment), request.Id);

            return ManagerDriverAssignmentDetailDTO.FromEntity(assignment);
        }
    }
}
