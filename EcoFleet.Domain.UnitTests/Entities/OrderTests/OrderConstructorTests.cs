using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.OrderTests;

public class OrderConstructorTests
{
    private static DriverId DefaultDriverId => new(Guid.NewGuid());
    private static Geolocation DefaultPickUp => Geolocation.Create(40.416, -3.703);
    private static Geolocation DefaultDropOff => Geolocation.Create(41.385, 2.173);

    [Fact]
    public void Constructor_ShouldSetAllFields()
    {
        var driverId = DefaultDriverId;

        var order = new Order(driverId, DefaultPickUp, DefaultDropOff, 25.50m);

        order.DriverId.Should().Be(driverId);
        order.PickUpLocation.Should().Be(DefaultPickUp);
        order.DropOffLocation.Should().Be(DefaultDropOff);
        order.Price.Should().Be(25.50m);
    }

    [Fact]
    public void Constructor_ShouldSetStatusToPending()
    {
        var order = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedDateToNow()
    {
        var before = DateTime.UtcNow;

        var order = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);

        var after = DateTime.UtcNow;
        order.CreatedDate.Should().BeOnOrAfter(before);
        order.CreatedDate.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_ShouldHaveNullFinishedDate()
    {
        var order = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);

        order.FinishedDate.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        var order = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);

        order.Id.Should().NotBeNull();
        order.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowDomainException()
    {
        var act = () => new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, -1m);

        act.Should().Throw<DomainException>()
            .WithMessage("Price cannot be negative.");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldSucceed()
    {
        var order = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 0m);

        order.Price.Should().Be(0m);
    }

    [Fact]
    public void Constructor_TwoOrders_ShouldHaveDifferentIds()
    {
        var order1 = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);
        var order2 = new Order(DefaultDriverId, DefaultPickUp, DefaultDropOff, 10m);

        order1.Id.Should().NotBe(order2.Id);
    }
}
