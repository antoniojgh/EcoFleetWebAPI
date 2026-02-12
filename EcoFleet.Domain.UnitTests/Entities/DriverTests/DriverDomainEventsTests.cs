using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverDomainEventsTests
{
    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));
        driver.Suspend();

        driver.DomainEvents.Should().NotBeEmpty();

        driver.ClearDomainEvents();

        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Suspend_EventShouldContainCorrectDriverId()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));

        driver.Suspend();

        var domainEvent = driver.DomainEvents.Single() as DriverSuspendedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.DriverId.Should().Be(driver.Id);
        domainEvent.OcurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Reinstate_EventShouldContainCorrectDriverId()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));
        driver.Suspend();
        driver.ClearDomainEvents();

        driver.Reinstate();

        var domainEvent = driver.DomainEvents.Single() as DriverReinstatedEvent;

        domainEvent.Should().NotBeNull();
        domainEvent!.DriverId.Should().Be(driver.Id);
        domainEvent.OcurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MultipleOperations_ShouldAccumulateEvents()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));

        driver.Suspend();
        driver.Reinstate();
        driver.Suspend();

        driver.DomainEvents.Should().HaveCount(3);
        driver.DomainEvents.OfType<DriverSuspendedEvent>().Should().HaveCount(2);
        driver.DomainEvents.OfType<DriverReinstatedEvent>().Should().HaveCount(1);
    }
}
