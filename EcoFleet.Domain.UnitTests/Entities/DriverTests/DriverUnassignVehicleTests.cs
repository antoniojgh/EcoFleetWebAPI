using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverUnassignVehicleTests
{
    private static Driver CreateOnDutyDriver()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"),
            vehicleId);
        driver.ClearDomainEvents();
        return driver;
    }

    private static Driver CreateSuspendedDriver()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"));
        driver.Suspend();
        driver.ClearDomainEvents();
        return driver;
    }

    [Fact]
    public void UnassignVehicle_WhenOnDuty_ShouldClearVehicleAndSetAvailable()
    {
        var driver = CreateOnDutyDriver();

        driver.UnassignVehicle();

        driver.CurrentVehicleId.Should().BeNull();
        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void UnassignVehicle_WhenAvailable_ShouldRemainAvailable()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"));

        driver.UnassignVehicle();

        driver.CurrentVehicleId.Should().BeNull();
        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void UnassignVehicle_WhenSuspended_ShouldNotChangeState()
    {
        var driver = CreateSuspendedDriver();

        driver.UnassignVehicle();

        driver.Status.Should().Be(DriverStatus.Suspended);
        driver.CurrentVehicleId.Should().BeNull();
    }
}
