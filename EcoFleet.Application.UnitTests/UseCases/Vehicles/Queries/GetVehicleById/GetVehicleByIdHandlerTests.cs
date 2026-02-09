using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetVehicleById;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Queries.GetVehicleById;

public class GetVehicleByIdHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly GetVehicleByIdHandler _handler;

    public GetVehicleByIdHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _handler = new GetVehicleByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var query = new GetVehicleByIdQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldReturnCorrectDTO()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(40.416, -3.703));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var query = new GetVehicleByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(vehicle.Id.Value);
        result.LicensePlate.Should().Be("ABC-123");
        result.Status.Should().Be(VehicleStatus.Idle);
        result.Latitude.Should().Be(40.416);
        result.Longitude.Should().Be(-3.703);
        result.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenVehicleHasDriver_ShouldReturnDriverIdInDTO()
    {
        var driverId = Guid.NewGuid();
        var vehicle = new Vehicle(
            LicensePlate.Create("XYZ-789"),
            Geolocation.Create(10, 20),
            new DriverId(driverId));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var query = new GetVehicleByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(VehicleStatus.Active);
        result.CurrentDriverId.Should().Be(driverId);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var vehicleGuid = Guid.NewGuid();
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var query = new GetVehicleByIdQuery(vehicleGuid);

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<VehicleId>(id => id.Value == vehicleGuid),
            Arg.Any<CancellationToken>());
    }
}
