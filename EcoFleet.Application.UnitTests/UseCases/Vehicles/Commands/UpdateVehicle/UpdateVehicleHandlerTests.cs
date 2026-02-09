using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateVehicleHandler _handler;

    public UpdateVehicleHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateVehicleHandler(_repository, _unitOfWork);
    }

    private Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("OLD-111"), Geolocation.Create(0, 0));

    private Vehicle CreateActiveVehicle(Guid driverId)
        => new(LicensePlate.Create("OLD-111"), Geolocation.Create(0, 0), new DriverId(driverId));

    private void SetupRepositoryReturns(Vehicle vehicle)
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 10, 20, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 10, 20, null);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldUpdatePlateAndLocation()
    {
        var vehicle = CreateIdleVehicle();
        SetupRepositoryReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "NEW-999", 52.520, 13.405, null);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.Plate.Value.Should().Be("NEW-999");
        vehicle.CurrentLocation.Latitude.Should().Be(52.520);
        vehicle.CurrentLocation.Longitude.Should().Be(13.405);
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldCallUpdateAndSaveChanges()
    {
        var vehicle = CreateIdleVehicle();
        SetupRepositoryReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "NEW-999", 10, 20, null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(vehicle, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WithNewDriver_ShouldAssignDriver()
    {
        var vehicle = CreateIdleVehicle();
        SetupRepositoryReturns(vehicle);
        var driverId = Guid.NewGuid();

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().NotBeNull();
        vehicle.CurrentDriverId!.Value.Should().Be(driverId);
        vehicle.Status.Should().Be(VehicleStatus.Active);
    }

    [Fact]
    public async Task Handle_WithDifferentDriver_ShouldReassignDriver()
    {
        var existingDriverId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(existingDriverId);
        SetupRepositoryReturns(vehicle);
        var newDriverId = Guid.NewGuid();

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, newDriverId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId!.Value.Should().Be(newDriverId);
    }

    [Fact]
    public async Task Handle_WithSameDriver_ShouldNotReassign()
    {
        var driverId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(driverId);
        vehicle.ClearDomainEvents();
        SetupRepositoryReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId!.Value.Should().Be(driverId);
        vehicle.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullDriver_WhenActiveVehicle_ShouldUnassign()
    {
        var driverId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(driverId);
        SetupRepositoryReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, null);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
    }

    [Fact]
    public async Task Handle_WithNullDriver_WhenIdleVehicle_ShouldNotCallUnassign()
    {
        var vehicle = CreateIdleVehicle();
        vehicle.ClearDomainEvents();
        SetupRepositoryReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, null);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        vehicle.DomainEvents.Should().BeEmpty();
    }
}
