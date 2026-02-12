using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.VehicleTests;

public class VehicleAssignDriverTests
{
    private static Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(40.416, -3.703));

    private static Vehicle CreateMaintenanceVehicle()
    {
        var vehicle = CreateIdleVehicle();
        vehicle.MarkForMaintenance();
        vehicle.ClearDomainEvents();
        return vehicle;
    }

    [Fact]
    public void AssignDriver_WhenIdle_ShouldSetDriverAndStatusActive()
    {
        var vehicle = CreateIdleVehicle();
        var driverId = new DriverId(Guid.NewGuid());

        vehicle.AssignDriver(driverId);

        vehicle.CurrentDriverId.Should().Be(driverId);
        vehicle.Status.Should().Be(VehicleStatus.Active);
    }

    [Fact]
    public void AssignDriver_WhenIdle_ShouldRaiseVehicleDriverAssignedEvent()
    {
        var vehicle = CreateIdleVehicle();
        var driverId = new DriverId(Guid.NewGuid());

        vehicle.AssignDriver(driverId);

        vehicle.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VehicleDriverAssignedEvent>()
            .Which.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void AssignDriver_WhenMaintenance_ShouldThrowDomainException()
    {
        var vehicle = CreateMaintenanceVehicle();
        var driverId = new DriverId(Guid.NewGuid());

        var act = () => vehicle.AssignDriver(driverId);

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot assign a driver to a vehicle in maintenance.");
    }

    [Fact]
    public void AssignDriver_WhenMaintenance_ShouldNotChangeState()
    {
        var vehicle = CreateMaintenanceVehicle();
        var driverId = new DriverId(Guid.NewGuid());

        try { vehicle.AssignDriver(driverId); } catch { }

        vehicle.Status.Should().Be(VehicleStatus.Maintenance);
        vehicle.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public void AssignDriver_WhenActive_ShouldReassignDriver()
    {
        var vehicle = CreateIdleVehicle();
        var firstDriver = new DriverId(Guid.NewGuid());
        var secondDriver = new DriverId(Guid.NewGuid());

        vehicle.AssignDriver(firstDriver);
        vehicle.ClearDomainEvents();
        vehicle.AssignDriver(secondDriver);

        vehicle.CurrentDriverId.Should().Be(secondDriver);
        vehicle.Status.Should().Be(VehicleStatus.Active);
    }

    [Fact]
    public void AssignDriver_WhenActive_ShouldRaiseEventForEachAssignment()
    {
        var vehicle = CreateIdleVehicle();
        var firstDriver = new DriverId(Guid.NewGuid());
        var secondDriver = new DriverId(Guid.NewGuid());

        vehicle.AssignDriver(firstDriver);
        vehicle.AssignDriver(secondDriver);

        vehicle.DomainEvents.Should().HaveCount(2);
        vehicle.DomainEvents.Should().AllBeOfType<VehicleDriverAssignedEvent>();
    }

    [Fact]
    public void AssignDriver_ShouldRaiseEvent_WithCorrectValues()
    {
        var vehicle = CreateIdleVehicle();
        var driverId = new DriverId(Guid.NewGuid());

        vehicle.AssignDriver(driverId);

        var domainEvent = vehicle.DomainEvents.OfType<VehicleDriverAssignedEvent>().Single();
        domainEvent.VehicleId.Should().Be(vehicle.Id);
        domainEvent.DriverId.Should().Be(driverId);
        domainEvent.OcurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
