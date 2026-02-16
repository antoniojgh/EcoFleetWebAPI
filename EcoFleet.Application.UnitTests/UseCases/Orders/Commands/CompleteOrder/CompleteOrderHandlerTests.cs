using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Commands.CompleteOrder;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Commands.CompleteOrder;

public class CompleteOrderHandlerTests
{
    private readonly IRepositoryOrder _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CompleteOrderHandler _handler;

    public CompleteOrderHandlerTests()
    {
        _repository = Substitute.For<IRepositoryOrder>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CompleteOrderHandler(_repository, _unitOfWork);
    }

    private static Order CreateInProgressOrder()
    {
        var order = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);
        order.StartProgress();
        return order;
    }

    private static Order CreatePendingOrder()
        => new(new DriverId(Guid.NewGuid()), Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new CompleteOrderCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new CompleteOrderCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderIsInProgress_ShouldComplete()
    {
        var order = CreateInProgressOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CompleteOrderCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Completed);
        order.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsInProgress_ShouldCallUpdateAndSaveChanges()
    {
        var order = CreateInProgressOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CompleteOrderCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(order, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenOrderIsPending_ShouldThrowDomainException()
    {
        var order = CreatePendingOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CompleteOrderCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Only in-progress orders can be completed.");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var orderGuid = Guid.NewGuid();
        var order = CreateInProgressOrder();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new CompleteOrderCommand(orderGuid);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<OrderId>(id => id.Value == orderGuid),
            Arg.Any<CancellationToken>());
    }
}
