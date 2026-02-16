using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverUpdateTests
{
    private static Driver CreateDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com"));

    [Fact]
    public void UpdateName_ShouldUpdateName()
    {
        var driver = CreateDriver();
        var newName = FullName.Create("Jane", "Smith");

        driver.UpdateName(newName);

        driver.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_ShouldPersistNewValues()
    {
        var driver = CreateDriver();

        driver.UpdateName(FullName.Create("Alice", "Johnson"));

        driver.Name.FirstName.Should().Be("Alice");
        driver.Name.LastName.Should().Be("Johnson");
    }

    [Fact]
    public void UpdateLicense_ShouldUpdateLicense()
    {
        var driver = CreateDriver();
        var newLicense = DriverLicense.Create("DL-999999");

        driver.UpdateLicense(newLicense);

        driver.License.Should().Be(newLicense);
    }

    [Fact]
    public void UpdateLicense_ShouldPersistNewValue()
    {
        var driver = CreateDriver();

        driver.UpdateLicense(DriverLicense.Create("new-abc"));

        driver.License.Value.Should().Be("NEW-ABC");
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail()
    {
        var driver = CreateDriver();
        var newEmail = Email.Create("newemail@example.com");

        driver.UpdateEmail(newEmail);

        driver.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdatePhoneNumber()
    {
        var driver = CreateDriver();
        var phone = PhoneNumber.Create("+1234567890");

        driver.UpdatePhoneNumber(phone);

        driver.PhoneNumber.Should().Be(phone);
    }

    [Fact]
    public void UpdatePhoneNumber_WithNull_ShouldClearPhoneNumber()
    {
        var driver = CreateDriver();
        driver.UpdatePhoneNumber(PhoneNumber.Create("+1234567890"));

        driver.UpdatePhoneNumber(null);

        driver.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void UpdateDateOfBirth_ShouldUpdateDateOfBirth()
    {
        var driver = CreateDriver();
        var dob = new DateTime(1990, 5, 15);

        driver.UpdateDateOfBirth(dob);

        driver.DateOfBirth.Should().Be(dob);
    }

    [Fact]
    public void UpdateDateOfBirth_WithNull_ShouldClearDateOfBirth()
    {
        var driver = CreateDriver();
        driver.UpdateDateOfBirth(new DateTime(1990, 5, 15));

        driver.UpdateDateOfBirth(null);

        driver.DateOfBirth.Should().BeNull();
    }

    [Fact]
    public void UpdateDateOfBirth_WithFutureDate_ShouldThrowDomainException()
    {
        var driver = CreateDriver();
        var futureDate = DateTime.UtcNow.Date.AddDays(1);

        var act = () => driver.UpdateDateOfBirth(futureDate);

        act.Should().Throw<DomainException>()
            .WithMessage("Date of birth cannot be in the future.");
    }
}
