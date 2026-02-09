using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Commands.DeleteVehicle;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.DeleteVehicle;

public class DeleteVehicleHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteVehicleHandler _handler;

    public DeleteVehicleHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteVehicleHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new DeleteVehicleCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldNotCallDelete()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new DeleteVehicleCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Delete(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldCallDelete()
    {
        var vehicle = new Vehicle(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new DeleteVehicleCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).Delete(vehicle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldCallSaveChanges()
    {
        var vehicle = new Vehicle(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new DeleteVehicleCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldCallDeleteBeforeSaveChanges()
    {
        var vehicle = new Vehicle(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);

        var command = new DeleteVehicleCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Delete(vehicle, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_NotFoundMessage_ShouldContainEntityNameAndId()
    {
        _repository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var vehicleGuid = Guid.NewGuid();
        var command = new DeleteVehicleCommand(vehicleGuid);

        var act = () => _handler.Handle(command, CancellationToken.None);

        (await act.Should().ThrowAsync<NotFoundException>())
            .WithMessage($"*Vehicle*{vehicleGuid}*");
    }
}
