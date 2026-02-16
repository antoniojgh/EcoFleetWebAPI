using EcoFleet.Application.UseCases.Orders.Queries.DTOs;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Queries.DTOs;

public class OrderDetailDTOTests
{
    [Fact]
    public void FromEntity_WithPendingOrder_ShouldMapAllFieldsCorrectly()
    {
        var driverId = new DriverId(Guid.NewGuid());
        var order = new Order(driverId, Geolocation.Create(40.416, -3.703), Geolocation.Create(41.385, 2.173), 25.50m);

        var dto = OrderDetailDTO.FromEntity(order);

        dto.Id.Should().Be(order.Id.Value);
        dto.DriverId.Should().Be(driverId.Value);
        dto.Status.Should().Be(OrderStatus.Pending);
        dto.PickUpLatitude.Should().Be(40.416);
        dto.PickUpLongitude.Should().Be(-3.703);
        dto.DropOffLatitude.Should().Be(41.385);
        dto.DropOffLongitude.Should().Be(2.173);
        dto.Price.Should().Be(25.50m);
        dto.FinishedDate.Should().BeNull();
    }

    [Fact]
    public void FromEntity_WithCompletedOrder_ShouldMapFinishedDate()
    {
        var order = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m);
        order.StartProgress();
        order.Complete();

        var dto = OrderDetailDTO.FromEntity(order);

        dto.Status.Should().Be(OrderStatus.Completed);
        dto.FinishedDate.Should().NotBeNull();
    }

    [Fact]
    public void FromEntity_IdShouldMatchEntityId()
    {
        var order = new Order(new DriverId(Guid.NewGuid()), Geolocation.Create(0, 0), Geolocation.Create(1, 1), 10m);

        var dto = OrderDetailDTO.FromEntity(order);

        dto.Id.Should().Be(order.Id.Value);
        dto.Id.Should().NotBe(Guid.Empty);
    }
}
