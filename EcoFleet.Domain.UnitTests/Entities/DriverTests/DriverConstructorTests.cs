using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverConstructorTests
{
    private static FullName DefaultName => FullName.Create("John", "Doe");
    private static DriverLicense DefaultLicense => DriverLicense.Create("DL-123456");
    private static Email DefaultEmail => Email.Create("john@example.com");

    [Fact]
    public void Constructor_WithoutVehicle_ShouldSetStatusToAvailable()
    {
        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldSetNameLicenseAndEmail()
    {
        var name = DefaultName;
        var license = DefaultLicense;
        var email = DefaultEmail;

        var driver = new Driver(name, license, email);

        driver.Name.Should().Be(name);
        driver.License.Should().Be(license);
        driver.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldHaveNullVehicleId()
    {
        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldGenerateUniqueId()
    {
        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver.Id.Should().NotBeNull();
        driver.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithoutVehicle_ShouldNotRaiseDomainEvents()
    {
        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithOptionalFields_ShouldSetPhoneNumberAndDateOfBirth()
    {
        var phone = PhoneNumber.Create("+1234567890");
        var dob = new DateTime(1990, 5, 15);

        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail, phone, dob);

        driver.PhoneNumber.Should().Be(phone);
        driver.DateOfBirth.Should().Be(dob);
    }

    [Fact]
    public void Constructor_WithoutOptionalFields_ShouldHaveNullPhoneAndDateOfBirth()
    {
        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver.PhoneNumber.Should().BeNull();
        driver.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFutureDateOfBirth_ShouldThrowDomainException()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        var act = () => new Driver(DefaultName, DefaultLicense, DefaultEmail, null, futureDate);

        act.Should().Throw<DomainException>()
            .WithMessage("Date of birth cannot be in the future.");
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldSetStatusToOnDuty()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail, vehicleId);

        driver.Status.Should().Be(DriverStatus.OnDuty);
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldAssignVehicle()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail, vehicleId);

        driver.CurrentVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public void Constructor_WithVehicle_ShouldNotRaiseDomainEvents()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());

        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail, vehicleId);

        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithVehicleAndOptionalFields_ShouldSetAllFields()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());
        var phone = PhoneNumber.Create("+1234567890");
        var dob = new DateTime(1990, 5, 15);

        var driver = new Driver(DefaultName, DefaultLicense, DefaultEmail, vehicleId, phone, dob);

        driver.CurrentVehicleId.Should().Be(vehicleId);
        driver.PhoneNumber.Should().Be(phone);
        driver.DateOfBirth.Should().Be(dob);
        driver.Status.Should().Be(DriverStatus.OnDuty);
    }

    [Fact]
    public void Constructor_TwoDrivers_ShouldHaveDifferentIds()
    {
        var driver1 = new Driver(DefaultName, DefaultLicense, DefaultEmail);
        var driver2 = new Driver(DefaultName, DefaultLicense, DefaultEmail);

        driver1.Id.Should().NotBe(driver2.Id);
    }
}
