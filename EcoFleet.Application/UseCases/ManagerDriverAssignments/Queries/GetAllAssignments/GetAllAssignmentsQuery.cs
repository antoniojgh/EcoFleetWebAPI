using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAllAssignments
{
    public record GetAllAssignmentsQuery : FilterManagerDriverAssignmentDTO, IRequest<PaginatedDTO<ManagerDriverAssignmentDetailDTO>>;
}
