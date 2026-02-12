using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverUpdateTests
{
    private static Driver CreateDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

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
}
