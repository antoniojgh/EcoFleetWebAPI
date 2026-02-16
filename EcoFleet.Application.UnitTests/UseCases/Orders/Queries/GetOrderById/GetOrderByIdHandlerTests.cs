using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Queries.GetOrderById;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Queries.GetOrderById;

public class GetOrderByIdHandlerTests
{
    private readonly IRepositoryOrder _repository;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _repository = Substitute.For<IRepositoryOrder>();
        _handler = new GetOrderByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var query = new GetOrderByIdQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenOrderExists_ShouldReturnCorrectDTO()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var order = new Order(driverId, Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(order.Id.Value);
        result.DriverId.Should().Be(driverId.Value);
        result.Status.Should().Be(OrderStatus.Pending);
        result.PickUpLatitude.Should().Be(40.416);
        result.PickUpLongitude.Should().Be(-3.703);
        result.DropOffLatitude.Should().Be(41.385);
        result.DropOffLongitude.Should().Be(2.173);
        result.Price.Should().Be(25.50m);
        result.FinishedDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrderIsCompleted_ShouldReturnFinishedDate()
    {
        var order = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m);
        order.StartProgress();
        order.Complete();
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(OrderStatus.Completed);
        result.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var orderGuid = Guid.NewGuid();
        var order = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m);
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns(order);

        var query = new GetOrderByIdQuery(orderGuid);

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<OrderId>(id => id.Value == orderGuid),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFoundMessage_ShouldContainEntityNameAndId()
    {
        _repository.GetByIdAsync(Arg.Any<OrderId>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var id = Guid.NewGuid();
        var query = new GetOrderByIdQuery(id);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Order*{id}*");
    }
}
