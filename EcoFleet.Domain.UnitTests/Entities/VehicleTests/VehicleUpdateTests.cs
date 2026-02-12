using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.VehicleTests;

public class VehicleUpdateTests
{
    private static Vehicle CreateVehicle()
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(40.416, -3.703));

    [Fact]
    public void UpdateTelemetry_ShouldUpdateLocation()
    {
        var vehicle = CreateVehicle();
        var newLocation = Geolocation.Create(41.0, -4.0);

        vehicle.UpdateTelemetry(newLocation);

        vehicle.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public void UpdateTelemetry_ShouldPersistNewCoordinateValues()
    {
        var vehicle = CreateVehicle();
        var newLocation = Geolocation.Create(52.520, 13.405);

        vehicle.UpdateTelemetry(newLocation);

        vehicle.CurrentLocation.Latitude.Should().Be(52.520);
        vehicle.CurrentLocation.Longitude.Should().Be(13.405);
    }

    [Fact]
    public void UpdatePlate_ShouldUpdatePlate()
    {
        var vehicle = CreateVehicle();
        var newPlate = LicensePlate.Create("XYZ-789");

        vehicle.UpdatePlate(newPlate);

        vehicle.Plate.Should().Be(newPlate);
    }

    [Fact]
    public void UpdatePlate_ShouldPersistNewPlateValue()
    {
        var vehicle = CreateVehicle();

        vehicle.UpdatePlate(LicensePlate.Create("new-999"));

        vehicle.Plate.Value.Should().Be("NEW-999");
    }
}
