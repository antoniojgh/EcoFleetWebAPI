using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.CreateVehicle;

public class CreateVehicleHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateVehicleHandler _handler;

    public CreateVehicleHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateVehicleHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithoutDriver_ShouldReturnNonEmptyGuid()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithoutDriver_ShouldCallAddAsync()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.Plate.Value == "ABC-123" &&
            v.CurrentLocation.Latitude == 40.416 &&
            v.CurrentLocation.Longitude == -3.703 &&
            v.Status == VehicleStatus.Idle &&
            v.CurrentDriverId == null));
    }

    [Fact]
    public async Task Handle_WithoutDriver_ShouldCallSaveChanges()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDriver_ShouldCreateActiveVehicle()
    {
        var driverId = Guid.NewGuid();
        var command = new CreateVehicleCommand("XYZ-789", 10.0, 20.0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.Status == VehicleStatus.Active &&
            v.CurrentDriverId != null &&
            v.CurrentDriverId.Value == driverId));
    }

    [Fact]
    public async Task Handle_WithDriver_ShouldReturnNonEmptyGuid()
    {
        var driverId = Guid.NewGuid();
        var command = new CreateVehicleCommand("XYZ-789", 10.0, 20.0, driverId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddBeforeSaveChanges()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.AddAsync(Arg.Any<Vehicle>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_TwoInvocations_ShouldReturnDifferentIds()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        result1.Should().NotBe(result2);
    }

    [Fact]
    public async Task Handle_WithLowercasePlate_ShouldConvertToUpper()
    {
        var command = new CreateVehicleCommand("abc-xyz", 0, 0, null);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.Plate.Value == "ABC-XYZ"));
    }
}
