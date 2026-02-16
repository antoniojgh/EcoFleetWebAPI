using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.ManagerTests;

public class ManagerConstructorTests
{
    private static FullName DefaultName => FullName.Create("Alice", "Manager");
    private static Email DefaultEmail => Email.Create("alice@example.com");

    [Fact]
    public void Constructor_ShouldSetNameAndEmail()
    {
        var name = DefaultName;
        var email = DefaultEmail;

        var manager = new Manager(name, email);

        manager.Name.Should().Be(name);
        manager.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        var manager = new Manager(DefaultName, DefaultEmail);

        manager.Id.Should().NotBeNull();
        manager.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_TwoManagers_ShouldHaveDifferentIds()
    {
        var manager1 = new Manager(DefaultName, DefaultEmail);
        var manager2 = new Manager(DefaultName, DefaultEmail);

        manager1.Id.Should().NotBe(manager2.Id);
    }

    [Fact]
    public void Constructor_ShouldNotRaiseDomainEvents()
    {
        var manager = new Manager(DefaultName, DefaultEmail);

        manager.DomainEvents.Should().BeEmpty();
    }
}
