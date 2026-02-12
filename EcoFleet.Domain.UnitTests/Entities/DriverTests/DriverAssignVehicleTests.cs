using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverAssignVehicleTests
{
    private static Driver CreateAvailableDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

    private static Driver CreateSuspendedDriver()
    {
        var driver = CreateAvailableDriver();
        driver.Suspend();
        driver.ClearDomainEvents();
        return driver;
    }

    [Fact]
    public void AssignVehicle_WhenAvailable_ShouldSetVehicleAndStatusOnDuty()
    {
        var driver = CreateAvailableDriver();
        var vehicleId = new VehicleId(Guid.NewGuid());

        driver.AssignVehicle(vehicleId);

        driver.CurrentVehicleId.Should().Be(vehicleId);
        driver.Status.Should().Be(DriverStatus.OnDuty);
    }

    [Fact]
    public void AssignVehicle_WhenSuspended_ShouldThrowDomainException()
    {
        var driver = CreateSuspendedDriver();
        var vehicleId = new VehicleId(Guid.NewGuid());

        var act = () => driver.AssignVehicle(vehicleId);

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot assign a vehicle to a suspended driver.");
    }

    [Fact]
    public void AssignVehicle_WhenSuspended_ShouldNotChangeState()
    {
        var driver = CreateSuspendedDriver();
        var vehicleId = new VehicleId(Guid.NewGuid());

        try { driver.AssignVehicle(vehicleId); } catch { }

        driver.Status.Should().Be(DriverStatus.Suspended);
        driver.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void AssignVehicle_WhenOnDuty_ShouldReassignVehicle()
    {
        var driver = CreateAvailableDriver();
        var firstVehicle = new VehicleId(Guid.NewGuid());
        var secondVehicle = new VehicleId(Guid.NewGuid());

        driver.AssignVehicle(firstVehicle);
        driver.AssignVehicle(secondVehicle);

        driver.CurrentVehicleId.Should().Be(secondVehicle);
        driver.Status.Should().Be(DriverStatus.OnDuty);
    }
}
