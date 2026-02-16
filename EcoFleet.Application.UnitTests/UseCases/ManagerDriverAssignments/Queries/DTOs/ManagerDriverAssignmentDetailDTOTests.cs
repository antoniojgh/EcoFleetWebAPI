using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.DTOs;
using EcoFleet.Domain.Entities;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Queries.DTOs;

public class ManagerDriverAssignmentDetailDTOTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFieldsCorrectly()
    {
        var managerId = new ManagerId(Guid.NewGuid());
        var driverId = new DriverId(Guid.NewGuid());
        var assignment = new ManagerDriverAssignment(managerId, driverId);

        var dto = ManagerDriverAssignmentDetailDTO.FromEntity(assignment);

        dto.Id.Should().Be(assignment.Id.Value);
        dto.ManagerId.Should().Be(managerId.Value);
        dto.DriverId.Should().Be(driverId.Value);
        dto.AssignedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FromEntity_WhenDeactivated_ShouldMapIsActiveFalse()
    {
        var assignment = new ManagerDriverAssignment(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()));
        assignment.Deactivate();

        var dto = ManagerDriverAssignmentDetailDTO.FromEntity(assignment);

        dto.IsActive.Should().BeFalse();
    }
}
