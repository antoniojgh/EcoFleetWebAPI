using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Orders.Queries.GetAllOrders;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Queries.GetAllOrders;

public class GetAllOrdersHandlerTests
{
    private readonly IRepositoryOrder _repository;
    private readonly GetAllOrdersHandler _handler;

    public GetAllOrdersHandlerTests()
    {
        _repository = Substitute.For<IRepositoryOrder>();
        _handler = new GetAllOrdersHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNoOrders_ShouldReturnEmptyPaginatedResult()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterOrderDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Order>());

        var query = new GetAllOrdersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenOrdersExist_ShouldReturnCorrectCount()
    {
        var orders = new List<Order>
        {
            new(new DriverId(Guid.NewGuid()), Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m),
            new(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m)
        };
        _repository.GetFilteredAsync(Arg.Any<FilterOrderDTO>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var query = new GetAllOrdersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapOrdersToDTOsCorrectly()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var pendingOrder = new Order(driverId, Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);

        var completedOrder = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m);
        completedOrder.StartProgress();
        completedOrder.Complete();

        var orders = new List<Order> { pendingOrder, completedOrder };
        _repository.GetFilteredAsync(Arg.Any<FilterOrderDTO>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var query = new GetAllOrdersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var items = result.Items.ToList();
        items[0].DriverId.Should().Be(driverId.Value);
        items[0].Status.Should().Be(OrderStatus.Pending);
        items[0].Price.Should().Be(25.50m);
        items[0].PickUpLatitude.Should().Be(40.416);
        items[0].PickUpLongitude.Should().Be(-3.703);
        items[0].FinishedDate.Should().BeNull();

        items[1].Status.Should().Be(OrderStatus.Completed);
        items[1].FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldPassQueryAsFilterToRepository()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterOrderDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Order>());

        var query = new GetAllOrdersQuery();

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetFilteredAsync(query, Arg.Any<CancellationToken>());
    }
}
