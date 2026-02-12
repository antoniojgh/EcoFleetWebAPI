using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.VehicleTests;

public class VehicleUnassignDriverTests
{
    private static Vehicle CreateActiveVehicle()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703),
            driverId);
        vehicle.ClearDomainEvents();
        return vehicle;
    }

    private static Vehicle CreateMaintenanceVehicle()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));
        vehicle.MarkForMaintenance();
        vehicle.ClearDomainEvents();
        return vehicle;
    }

    [Fact]
    public void UnassignDriver_WhenActive_ShouldClearDriverAndSetIdle()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.UnassignDriver();

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public void UnassignDriver_WhenIdle_ShouldRemainIdle()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));

        vehicle.UnassignDriver();

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public void UnassignDriver_WhenMaintenance_ShouldNotChangeState()
    {
        var vehicle = CreateMaintenanceVehicle();

        vehicle.UnassignDriver();

        vehicle.Status.Should().Be(VehicleStatus.Maintenance);
        vehicle.CurrentDriverId.Should().BeNull();
    }
}
