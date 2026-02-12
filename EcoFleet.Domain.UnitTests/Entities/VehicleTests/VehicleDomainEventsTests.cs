using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.VehicleTests;

public class VehicleDomainEventsTests
{
    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703),
            driverId);

        vehicle.DomainEvents.Should().NotBeEmpty();

        vehicle.ClearDomainEvents();

        vehicle.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithDriver_EventShouldContainCorrectVehicleId()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703),
            driverId);

        var domainEvent = vehicle.DomainEvents.Single() as VehicleDriverAssignedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.VehicleId.Should().Be(vehicle.Id);
        domainEvent.DriverId.Should().Be(driverId);
        domainEvent.OcurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkForMaintenance_EventShouldContainCorrectVehicleId()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));

        vehicle.MarkForMaintenance();

        var domainEvent = vehicle.DomainEvents.Single() as VehicleMaintenanceStartedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.VehicleId.Should().Be(vehicle.Id);
        domainEvent.OcurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MultipleOperations_ShouldAccumulateEvents()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));

        var driverId = new DriverId(Guid.NewGuid());
        vehicle.AssignDriver(driverId);
        vehicle.UnassignDriver();
        vehicle.MarkForMaintenance();

        vehicle.DomainEvents.Should().HaveCount(2);
        vehicle.DomainEvents.OfType<VehicleDriverAssignedEvent>().Should().HaveCount(1);
        vehicle.DomainEvents.OfType<VehicleMaintenanceStartedEvent>().Should().HaveCount(1);
    }
}
