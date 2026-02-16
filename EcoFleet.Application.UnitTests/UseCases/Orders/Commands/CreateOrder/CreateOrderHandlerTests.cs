using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Orders.Commands.CreateOrder;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderHandlerTests
{
    private readonly IRepositoryOrder _orderRepository;
    private readonly IRepositoryDriver _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepository = Substitute.For<IRepositoryOrder>();
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateOrderHandler(_orderRepository, _driverRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidDriver_ShouldReturnNonEmptyGuid()
    {
        var driverId = Guid.NewGuid();
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com")));

        var command = new CreateOrderCommand(driverId, 40.416, -3.703, 41.385, 2.173, 25.50m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidDriver_ShouldCallAddAndSaveChanges()
    {
        var driverId = Guid.NewGuid();
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com")));

        var command = new CreateOrderCommand(driverId, 40.416, -3.703, 41.385, 2.173, 25.50m);

        await _handler.Handle(command, CancellationToken.None);

        await _orderRepository.Received(1).AddAsync(Arg.Is<Order>(o =>
            o.Status == OrderStatus.Pending &&
            o.Price == 25.50m), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDriver_ShouldThrowNotFoundException()
    {
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new CreateOrderCommand(Guid.NewGuid(), 40.416, -3.703, 41.385, 2.173, 25.50m);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentDriver_ShouldNotCreateOrder()
    {
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new CreateOrderCommand(Guid.NewGuid(), 40.416, -3.703, 41.385, 2.173, 25.50m);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
