using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverReinstateTests
{
    private static Driver CreateSuspendedDriver()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));
        driver.Suspend();
        driver.ClearDomainEvents();
        return driver;
    }

    [Fact]
    public void Reinstate_WhenSuspended_ShouldSetStatusToAvailable()
    {
        var driver = CreateSuspendedDriver();

        driver.Reinstate();

        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public void Reinstate_WhenSuspended_ShouldRaiseDriverReinstatedEvent()
    {
        var driver = CreateSuspendedDriver();

        driver.Reinstate();

        driver.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DriverReinstatedEvent>()
            .Which.DriverId.Should().Be(driver.Id);
    }

    [Fact]
    public void Reinstate_WhenAvailable_ShouldThrowDomainException()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));

        var act = () => driver.Reinstate();

        act.Should().Throw<DomainException>()
            .WithMessage("Only suspended drivers can be reinstated.");
    }

    [Fact]
    public void Reinstate_WhenOnDuty_ShouldThrowDomainException()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            new VehicleId(Guid.NewGuid()));

        var act = () => driver.Reinstate();

        act.Should().Throw<DomainException>()
            .WithMessage("Only suspended drivers can be reinstated.");
    }

    [Fact]
    public void Reinstate_WhenNotSuspended_ShouldNotChangeState()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));

        try { driver.Reinstate(); } catch { }

        driver.Status.Should().Be(DriverStatus.Available);
        driver.DomainEvents.Should().BeEmpty();
    }
}
