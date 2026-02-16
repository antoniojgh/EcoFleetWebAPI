using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Entities.OrderTests;

public class OrderBehaviorTests
{
    private static Order CreatePendingOrder()
        => new(new DriverId(Guid.NewGuid()), Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);

    private static Order CreateInProgressOrder()
    {
        var order = CreatePendingOrder();
        order.StartProgress();
        return order;
    }

    private static Order CreateCompletedOrder()
    {
        var order = CreateInProgressOrder();
        order.Complete();
        return order;
    }

    // --- StartProgress ---

    [Fact]
    public void StartProgress_WhenPending_ShouldSetStatusToInProgress()
    {
        var order = CreatePendingOrder();

        order.StartProgress();

        order.Status.Should().Be(OrderStatus.InProgress);
    }

    [Fact]
    public void StartProgress_WhenNotPending_ShouldThrowDomainException()
    {
        var order = CreateInProgressOrder();

        var act = () => order.StartProgress();

        act.Should().Throw<DomainException>()
            .WithMessage("Only pending orders can be started.");
    }

    // --- Complete ---

    [Fact]
    public void Complete_WhenInProgress_ShouldSetStatusToCompleted()
    {
        var order = CreateInProgressOrder();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldSetFinishedDate()
    {
        var order = CreateInProgressOrder();
        var before = DateTime.UtcNow;

        order.Complete();

        var after = DateTime.UtcNow;
        order.FinishedDate.Should().NotBeNull();
        order.FinishedDate!.Value.Should().BeOnOrAfter(before);
        order.FinishedDate!.Value.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Complete_WhenPending_ShouldThrowDomainException()
    {
        var order = CreatePendingOrder();

        var act = () => order.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("Only in-progress orders can be completed.");
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_WhenPending_ShouldSetStatusToCancelled()
    {
        var order = CreatePendingOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSetFinishedDate()
    {
        var order = CreatePendingOrder();

        order.Cancel();

        order.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenInProgress_ShouldSetStatusToCancelled()
    {
        var order = CreateInProgressOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowDomainException()
    {
        var order = CreateCompletedOrder();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("Cannot cancel a completed order.");
    }

    // --- UpdatePrice ---

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        var order = CreatePendingOrder();

        order.UpdatePrice(50m);

        order.Price.Should().Be(50m);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrowDomainException()
    {
        var order = CreatePendingOrder();

        var act = () => order.UpdatePrice(-1m);

        act.Should().Throw<DomainException>()
            .WithMessage("Price cannot be negative.");
    }

    [Fact]
    public void UpdatePrice_WithZero_ShouldSucceed()
    {
        var order = CreatePendingOrder();

        order.UpdatePrice(0m);

        order.Price.Should().Be(0m);
    }
}
