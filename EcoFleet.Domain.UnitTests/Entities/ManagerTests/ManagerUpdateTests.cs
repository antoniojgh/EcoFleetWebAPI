using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.ManagerTests;

public class ManagerUpdateTests
{
    private static Manager CreateManager()
        => new(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com"));

    [Fact]
    public void UpdateName_ShouldUpdateName()
    {
        var manager = CreateManager();
        var newName = FullName.Create("Bob", "Smith");

        manager.UpdateName(newName);

        manager.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_ShouldPersistNewValues()
    {
        var manager = CreateManager();

        manager.UpdateName(FullName.Create("Charlie", "Brown"));

        manager.Name.FirstName.Should().Be("Charlie");
        manager.Name.LastName.Should().Be("Brown");
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateEmail()
    {
        var manager = CreateManager();
        var newEmail = Email.Create("newemail@example.com");

        manager.UpdateEmail(newEmail);

        manager.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdateEmail_ShouldPersistNewValue()
    {
        var manager = CreateManager();

        manager.UpdateEmail(Email.Create("updated@company.com"));

        manager.Email.Value.Should().Be("updated@company.com");
    }
}
