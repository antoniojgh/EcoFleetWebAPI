using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities;

public class VehicleConstructorTests
{
    private static LicensePlate DefaultPlate => LicensePlate.Create("ABC-123");
    private static Geolocation DefaultLocation => Geolocation.Create(40.416, -3.703);

    [Fact]
    public void Constructor_WithoutDriver_ShouldSetStatusToIdle()
    {
        var vehicle = new Vehicle(DefaultPlate, DefaultLocation);

        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public void Constructor_WithoutDriver_ShouldSetPlateAndLocation()
    {
        var plate = DefaultPlate;
        var location = DefaultLocation;

        var vehicle = new Vehicle(plate, location);

        vehicle.Plate.Should().Be(plate);
        vehicle.CurrentLocation.Should().Be(location);
    }

    [Fact]
    public void Constructor_WithoutDriver_ShouldHaveNullDriverId()
    {
        var vehicle = new Vehicle(DefaultPlate, DefaultLocation);

        vehicle.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutDriver_ShouldGenerateUniqueId()
    {
        var vehicle = new Vehicle(DefaultPlate, DefaultLocation);

        vehicle.Id.Should().NotBeNull();
        vehicle.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithoutDriver_ShouldNotRaiseDomainEvents()
    {
        var vehicle = new Vehicle(DefaultPlate, DefaultLocation);

        vehicle.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithDriver_ShouldSetStatusToActive()
    {
        var driverId = new DriverId(Guid.NewGuid());

        var vehicle = new Vehicle(DefaultPlate, DefaultLocation, driverId);

        vehicle.Status.Should().Be(VehicleStatus.Active);
    }

    [Fact]
    public void Constructor_WithDriver_ShouldAssignDriver()
    {
        var driverId = new DriverId(Guid.NewGuid());

        var vehicle = new Vehicle(DefaultPlate, DefaultLocation, driverId);

        vehicle.CurrentDriverId.Should().Be(driverId);
    }

    [Fact]
    public void Constructor_WithDriver_ShouldRaiseVehicleDriverAssignedEvent()
    {
        var driverId = new DriverId(Guid.NewGuid());

        var vehicle = new Vehicle(DefaultPlate, DefaultLocation, driverId);

        vehicle.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VehicleDriverAssignedEvent>()
            .Which.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void Constructor_TwoVehicles_ShouldHaveDifferentIds()
    {
        var vehicle1 = new Vehicle(DefaultPlate, DefaultLocation);
        var vehicle2 = new Vehicle(DefaultPlate, DefaultLocation);

        vehicle1.Id.Should().NotBe(vehicle2.Id);
    }
}
