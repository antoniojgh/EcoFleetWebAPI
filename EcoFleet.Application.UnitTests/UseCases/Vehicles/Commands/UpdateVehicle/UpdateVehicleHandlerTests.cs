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
    private readonly IRepositoryVehicle _vehicleRepository;
    private readonly IRepositoryDriver _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateVehicleHandler _handler;

    public UpdateVehicleHandlerTests()
    {
        _vehicleRepository = Substitute.For<IRepositoryVehicle>();
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateVehicleHandler(_vehicleRepository, _driverRepository, _unitOfWork);
    }

    private Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("OLD-111"), Geolocation.Create(0, 0));

    private Vehicle CreateActiveVehicle(Guid driverId)
        => new(LicensePlate.Create("OLD-111"), Geolocation.Create(0, 0), new DriverId(driverId));

    private Driver CreateAvailableDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

    private Driver CreateOnDutyDriver(VehicleId vehicleId)
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), vehicleId);

    private void SetupVehicleReturns(Vehicle vehicle)
    {
        _vehicleRepository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns(vehicle);
    }

    private void SetupDriverReturns(DriverId driverId, Driver driver)
    {
        _driverRepository.GetByIdAsync(Arg.Is<DriverId>(id => id == driverId), Arg.Any<CancellationToken>())
            .Returns(driver);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldThrowNotFoundException()
    {
        _vehicleRepository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 10, 20, null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ShouldNotCallUpdate()
    {
        _vehicleRepository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 10, 20, null);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _vehicleRepository.DidNotReceive().Update(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenVehicleExists_ShouldUpdatePlateAndLocation()
    {
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicle);

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
        SetupVehicleReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "NEW-999", 10, 20, null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _vehicleRepository.Update(vehicle, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WithAvailableDriver_ShouldAssignDriverAndSyncBoth()
    {
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicle);
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver();
        SetupDriverReturns(new DriverId(driverId), driver);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().NotBeNull();
        vehicle.CurrentDriverId!.Value.Should().Be(driverId);
        vehicle.Status.Should().Be(VehicleStatus.Active);
        driver.Status.Should().Be(DriverStatus.OnDuty);
        driver.CurrentVehicleId.Should().Be(vehicle.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentDriver_ShouldThrowNotFoundException()
    {
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicle);
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSuspendedDriver_ShouldThrowBusinessRuleException()
    {
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicle);
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver();
        driver.Suspend();
        SetupDriverReturns(new DriverId(driverId), driver);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, driverId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage($"*{driverId}*not available*");
    }

    [Fact]
    public async Task Handle_WithSameDriver_ShouldNotReassign()
    {
        var driverId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(driverId);
        vehicle.ClearDomainEvents();
        SetupVehicleReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId!.Value.Should().Be(driverId);
        vehicle.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullDriver_WhenActiveVehicle_ShouldUnassignBoth()
    {
        var driverId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(driverId);
        SetupVehicleReturns(vehicle);
        var driver = CreateOnDutyDriver(vehicle.Id);
        SetupDriverReturns(new DriverId(driverId), driver);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, null);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        driver.Status.Should().Be(DriverStatus.Available);
        driver.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNullDriver_WhenIdleVehicle_ShouldNotCallUnassign()
    {
        var vehicle = CreateIdleVehicle();
        vehicle.ClearDomainEvents();
        SetupVehicleReturns(vehicle);

        var command = new UpdateVehicleCommand(Guid.NewGuid(), "OLD-111", 0, 0, null);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.CurrentDriverId.Should().BeNull();
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        vehicle.DomainEvents.Should().BeEmpty();
    }
}
