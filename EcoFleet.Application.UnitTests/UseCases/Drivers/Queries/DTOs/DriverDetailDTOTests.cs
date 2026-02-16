using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Queries.DTOs;

public class DriverDetailDTOTests
{
    [Fact]
    public void FromEntity_WithAvailableDriver_ShouldMapAllFieldsCorrectly()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"),
            PhoneNumber.Create("+1234567890"),
            new DateTime(1990, 5, 15));

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.Id.Should().Be(driver.Id.Value);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.License.Should().Be("DL-123");
        dto.Email.Should().Be("john@example.com");
        dto.PhoneNumber.Should().Be("+1234567890");
        dto.DateOfBirth.Should().Be(new DateTime(1990, 5, 15));
        dto.Status.Should().Be(DriverStatus.Available);
        dto.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void FromEntity_WithOnDutyDriver_ShouldMapVehicleId()
    {
        var vehicleId = Guid.NewGuid();
        var driver = new Driver(
            FullName.Create("Jane", "Smith"),
            DriverLicense.Create("DL-789"),
            Email.Create("jane@example.com"),
            new VehicleId(vehicleId));

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.Status.Should().Be(DriverStatus.OnDuty);
        dto.CurrentVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public void FromEntity_WithSuspendedDriver_ShouldMapSuspendedStatus()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"));
        driver.Suspend();

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.Status.Should().Be(DriverStatus.Suspended);
        dto.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void FromEntity_ShouldPreserveLicenseValue()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("lower-case"),
            Email.Create("john@example.com"));

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.License.Should().Be("LOWER-CASE");
    }

    [Fact]
    public void FromEntity_WithNullOptionalFields_ShouldMapNulls()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"));

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.PhoneNumber.Should().BeNull();
        dto.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public void FromEntity_IdShouldMatchEntityId()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            Email.Create("john@example.com"));

        var dto = DriverDetailDTO.FromEntity(driver);

        dto.Id.Should().Be(driver.Id.Value);
        dto.Id.Should().NotBe(Guid.Empty);
    }
}
