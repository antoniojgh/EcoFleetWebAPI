using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Queries.DTOs;

public class VehicleDetailDTOTests
{
    [Fact]
    public void FromEntity_WithIdleVehicle_ShouldMapAllFieldsCorrectly()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));

        var dto = VehicleDetailDTO.FromEntity(vehicle);

        dto.Id.Should().Be(vehicle.Id.Value);
        dto.LicensePlate.Should().Be("ABC-123");
        dto.Status.Should().Be(VehicleStatus.Idle);
        dto.Latitude.Should().Be(40.416);
        dto.Longitude.Should().Be(-3.703);
        dto.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public void FromEntity_WithActiveVehicle_ShouldMapDriverId()
    {
        var driverId = Guid.NewGuid();
        var vehicle = new Vehicle(
            LicensePlate.Create("XYZ-789"),
            Geolocation.Create(10, 20),
            new DriverId(driverId));

        var dto = VehicleDetailDTO.FromEntity(vehicle);

        dto.Status.Should().Be(VehicleStatus.Active);
        dto.CurrentDriverId.Should().Be(driverId);
    }

    [Fact]
    public void FromEntity_WithMaintenanceVehicle_ShouldMapMaintenanceStatus()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("MNT-001"),
            Geolocation.Create(0, 0));
        vehicle.MarkForMaintenance();

        var dto = VehicleDetailDTO.FromEntity(vehicle);

        dto.Status.Should().Be(VehicleStatus.Maintenance);
        dto.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public void FromEntity_ShouldPreserveLicensePlateValue()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("lower-case"),
            Geolocation.Create(0, 0));

        var dto = VehicleDetailDTO.FromEntity(vehicle);

        dto.LicensePlate.Should().Be("LOWER-CASE");
    }

    [Fact]
    public void FromEntity_IdShouldMatchEntityId()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0));

        var dto = VehicleDetailDTO.FromEntity(vehicle);

        dto.Id.Should().NotBe(Guid.Empty);
        dto.Id.Should().Be(vehicle.Id.Value);
    }
}
