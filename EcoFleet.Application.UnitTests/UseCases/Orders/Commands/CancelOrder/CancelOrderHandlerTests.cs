using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Commands.CancelOrder;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Commands.CancelOrder;

public class CancelOrderHandlerTests
{
    private readonly IRepositoryOrder _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _repository = Substitute.For<IRepositoryOrder>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelOrderHandler(_repository, _unitOfWork);
    }

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

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new CancelOrderCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new CancelOrderCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsPending_ShouldCancel()
    {
        var order = CreatePendingOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CancelOrderCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsInProgress_ShouldCancel()
    {
        var order = CreateInProgressOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CancelOrderCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsCancelled_ShouldCallUpdateAndSaveChanges()
    {
        var order = CreatePendingOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CancelOrderCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(order, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenOrderIsCompleted_ShouldThrowDomainException()
    {
        var order = CreateCompletedOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CancelOrderCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot cancel a completed order.");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var orderGuid = Guid.NewGuid();
        var order = CreatePendingOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CancelOrderCommand(orderGuid);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<OrderId>(id => id.Value == orderGuid),
            Arg.Any<CancellationToken>());
    }
}
