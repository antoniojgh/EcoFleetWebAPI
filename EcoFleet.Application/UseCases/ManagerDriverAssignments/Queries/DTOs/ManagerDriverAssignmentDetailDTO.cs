using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs
{
    public record ManagerDriverAssignmentDetailDTO(
        Guid Id,
        Guid ManagerId,
        Guid DriverId,
        DateTime AssignedDate,
        bool IsActive
    )
    {
        public static ManagerDriverAssignmentDetailDTO FromEntity(ManagerDriverAssignment assignment) =>
            new(
                assignment.Id.Value,
                assignment.ManagerId.Value,
                assignment.DriverId.Value,
                assignment.AssignedDate,
                assignment.IsActive
            );
    }
}
