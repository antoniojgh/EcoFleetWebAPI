using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Exceptions;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.ManagerDriverAssignmentTests;

public class ManagerDriverAssignmentTests
{
    private static ManagerId DefaultManagerId => new(Guid.NewGuid());
    private static DriverId DefaultDriverId => new(Guid.NewGuid());

    [Fact]
    public void Constructor_ShouldSetManagerIdAndDriverId()
    {
        var managerId = DefaultManagerId;
        var driverId = DefaultDriverId;

        var assignment = new ManagerDriverAssignment(managerId, driverId);

        assignment.ManagerId.Should().Be(managerId);
        assignment.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void Constructor_ShouldSetIsActiveToTrue()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldSetAssignedDateToNow()
    {
        var before = DateTime.UtcNow;

        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        var after = DateTime.UtcNow;
        assignment.AssignedDate.Should().BeOnOrAfter(before);
        assignment.AssignedDate.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        assignment.Id.Should().NotBeNull();
        assignment.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_TwoAssignments_ShouldHaveDifferentIds()
    {
        var assignment1 = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);
        var assignment2 = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        assignment1.Id.Should().NotBe(assignment2.Id);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        assignment.Deactivate();

        assignment.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowDomainException()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);
        assignment.Deactivate();

        var act = () => assignment.Deactivate();

        act.Should().Throw<DomainException>()
            .WithMessage("Assignment is already inactive.");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrue()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);
        assignment.Deactivate();

        assignment.Activate();

        assignment.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowDomainException()
    {
        var assignment = new ManagerDriverAssignment(DefaultManagerId, DefaultDriverId);

        var act = () => assignment.Activate();

        act.Should().Throw<DomainException>()
            .WithMessage("Assignment is already active.");
    }
}
