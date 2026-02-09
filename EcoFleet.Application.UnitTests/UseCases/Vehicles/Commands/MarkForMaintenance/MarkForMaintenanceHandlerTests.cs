using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Commands.MarkForMaintenance;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.MarkForMaintenance;

public class MarkForMaintenanceHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly MarkForMaintenanceHandler _handler;

    public MarkForMaintenanceHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new MarkForMaintenanceHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleIsIdle_ShouldMarkForMaintenance()
    {
        var vehicle = new Vehicle(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        vehicle.Status.Should().Be(VehicleStatus.Maintenance);
    }

    [Fact]
    public async Task Handle_WhenVehicleIsIdle_ShouldCallUpdateAndSaveChanges()
    {
        var vehicle = new Vehicle(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(vehicle, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenVehicleIsActive_ShouldThrowDomainException()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0),
            new DriverId(Guid.NewGuid()));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot maintain a vehicle currently in use.");
    }

    [Fact]
    public async Task Handle_WhenVehicleIsActive_ShouldNotCallSaveChanges()
    {
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0),
            new DriverId(Guid.NewGuid()));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new MarkForMaintenanceCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
