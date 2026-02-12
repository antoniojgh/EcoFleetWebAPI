using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverConstructorTests
{
    private static FullName DefaultName => FullName.Create("John", "Doe");
    private static DriverLicense DefaultLicense => DriverLicense.Create("DL-123456");

    [Fact]
    public void Constructor_WithoutVehicle_ShouldSetStatusToAvailable()
    {
        var driver = new Driver(DefaultName, DefaultLicense);

        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldSetNameAndLicense()
    {
        var name = DefaultName;
        var license = DefaultLicense;

        var driver = new Driver(name, license);

        driver.Name.Should().Be(name);
        driver.License.Should().Be(license);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldHaveNullVehicleId()
    {
        var driver = new Driver(DefaultName, DefaultLicense);

        driver.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldGenerateUniqueId()
    {
        var driver = new Driver(DefaultName, DefaultLicense);

        driver.Id.Should().NotBeNull();
        driver.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldNotRaiseDomainEvents()
    {
        var driver = new Driver(DefaultName, DefaultLicense);

        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldSetStatusToOnDuty()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, vehicleId);

        driver.Status.Should().Be(DriverStatus.OnDuty);
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldAssignVehicle()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, vehicleId);

        driver.CurrentVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldNotRaiseDomainEvents()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, vehicleId);

        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_TwoDrivers_ShouldHaveDifferentIds()
    {
        var driver1 = new Driver(DefaultName, DefaultLicense);
        var driver2 = new Driver(DefaultName, DefaultLicense);

        driver1.Id.Should().NotBe(driver2.Id);
    }
}
