using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.DriverTests;

public class DriverSuspendTests
{
    private static Driver CreateAvailableDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

    private static Driver CreateOnDutyDriver()
    {
        var vehicleId = new VehicleId(Guid.NewGuid());
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            vehicleId);
        driver.ClearDomainEvents();
        return driver;
    }

    [Fact]
    public void Suspend_WhenAvailable_ShouldSetStatusToSuspended()
    {
        var driver = CreateAvailableDriver();

        driver.Suspend();

        driver.Status.Should().Be(DriverStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenAvailable_ShouldClearVehicleId()
    {
        var driver = CreateAvailableDriver();

        driver.Suspend();

        driver.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public void Suspend_WhenAvailable_ShouldRaiseDriverSuspendedEvent()
    {
        var driver = CreateAvailableDriver();

        driver.Suspend();

        driver.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<DriverSuspendedEvent>()
            .Which.DriverId.Should().Be(driver.Id);
    }

    [Fact]
    public void Suspend_WhenOnDuty_ShouldThrowDomainException()
    {
        var driver = CreateOnDutyDriver();

        var act = () => driver.Suspend();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot suspend a driver currently on duty.");
    }

    [Fact]
    public void Suspend_WhenOnDuty_ShouldNotChangeState()
    {
        var driver = CreateOnDutyDriver();

        try { driver.Suspend(); } catch { }

        driver.Status.Should().Be(DriverStatus.OnDuty);
        driver.CurrentVehicleId.Should().NotBeNull();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldRaiseAnotherEvent()
    {
        var driver = CreateAvailableDriver();

        driver.Suspend();
        driver.Suspend();

        driver.Status.Should().Be(DriverStatus.Suspended);
        driver.DomainEvents.OfType<DriverSuspendedEvent>().Should().HaveCount(2);
    }
}
