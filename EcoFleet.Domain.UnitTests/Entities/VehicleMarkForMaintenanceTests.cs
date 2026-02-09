using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities;

public class VehicleMarkForMaintenanceTests
{
    private static Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(40.416, -3.703));

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

    [Fact]
    public void MarkForMaintenance_WhenIdle_ShouldSetStatusToMaintenance()
    {
        var vehicle = CreateIdleVehicle();

        vehicle.MarkForMaintenance();

        vehicle.Status.Should().Be(VehicleStatus.Maintenance);
    }

    [Fact]
    public void MarkForMaintenance_WhenIdle_ShouldClearDriverId()
    {
        var vehicle = CreateIdleVehicle();

        vehicle.MarkForMaintenance();

        vehicle.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public void MarkForMaintenance_WhenIdle_ShouldRaiseVehicleMaintenanceStartedEvent()
    {
        var vehicle = CreateIdleVehicle();

        vehicle.MarkForMaintenance();

        vehicle.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VehicleMaintenanceStartedEvent>()
            .Which.VehicleId.Should().Be(vehicle.Id);
    }

    [Fact]
    public void MarkForMaintenance_WhenActive_ShouldThrowDomainException()
    {
        var vehicle = CreateActiveVehicle();

        var act = () => vehicle.MarkForMaintenance();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot maintain a vehicle currently in use.");
    }

    [Fact]
    public void MarkForMaintenance_WhenActive_ShouldNotChangeState()
    {
        var vehicle = CreateActiveVehicle();

        try { vehicle.MarkForMaintenance(); } catch { }

        vehicle.Status.Should().Be(VehicleStatus.Active);
        vehicle.CurrentDriverId.Should().NotBeNull();
    }
}
