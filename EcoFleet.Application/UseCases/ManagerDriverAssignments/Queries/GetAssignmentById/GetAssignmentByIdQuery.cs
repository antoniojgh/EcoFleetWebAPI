using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using MediatR;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAssignmentById
{
    public record GetAssignmentByIdQuery(Guid Id) : IRequest<ManagerDriverAssignmentDetailDTO>;
}
