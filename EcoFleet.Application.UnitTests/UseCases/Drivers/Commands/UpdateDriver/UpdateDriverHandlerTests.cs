using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Commands.UpdateDriver;

public class UpdateDriverHandlerTests
{
    private readonly IRepositoryDriver _driverRepository;
    private readonly IRepositoryVehicle _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateDriverHandler _handler;

    public UpdateDriverHandlerTests()
    {
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _vehicleRepository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateDriverHandler(_driverRepository, _vehicleRepository, _unitOfWork);
    }

    private Driver CreateAvailableDriver()
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

    private Driver CreateOnDutyDriver(Guid vehicleId)
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), new VehicleId(vehicleId));

    private Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));

    private Vehicle CreateActiveVehicle(DriverId driverId)
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0), driverId);

    private void SetupDriverReturns(Driver driver)
    {
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);
    }

    private void SetupVehicleReturns(VehicleId vehicleId, Vehicle vehicle)
    {
        _vehicleRepository.GetByIdAsync(Arg.Is<VehicleId>(id => id == vehicleId), Arg.Any<CancellationToken>())
            .Returns(vehicle);
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldThrowNotFoundException()
    {
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldNotCallUpdate()
    {
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", null);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _driverRepository.DidNotReceive().Update(Arg.Any<Driver>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldUpdateNameAndLicense()
    {
        var driver = CreateAvailableDriver();
        SetupDriverReturns(driver);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "Jane", "Smith", "DL-999", null);

        await _handler.Handle(command, CancellationToken.None);

        driver.Name.FirstName.Should().Be("Jane");
        driver.Name.LastName.Should().Be("Smith");
        driver.License.Value.Should().Be("DL-999");
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldCallUpdateAndSaveChanges()
    {
        var driver = CreateAvailableDriver();
        SetupDriverReturns(driver);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "Jane", "Smith", "DL-999", null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _driverRepository.Update(driver, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WithIdleVehicle_ShouldAssignVehicleAndSyncBoth()
    {
        var driver = CreateAvailableDriver();
        SetupDriverReturns(driver);
        var vehicleId = Guid.NewGuid();
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(new VehicleId(vehicleId), vehicle);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", vehicleId);

        await _handler.Handle(command, CancellationToken.None);

        driver.CurrentVehicleId.Should().NotBeNull();
        driver.CurrentVehicleId!.Value.Should().Be(vehicleId);
        driver.Status.Should().Be(DriverStatus.OnDuty);
        vehicle.Status.Should().Be(VehicleStatus.Active);
        vehicle.CurrentDriverId.Should().Be(driver.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentVehicle_ShouldThrowNotFoundException()
    {
        var driver = CreateAvailableDriver();
        SetupDriverReturns(driver);
        _vehicleRepository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithActiveVehicle_ShouldThrowBusinessRuleException()
    {
        var driver = CreateAvailableDriver();
        SetupDriverReturns(driver);
        var vehicleId = Guid.NewGuid();
        var vehicle = CreateActiveVehicle(new DriverId(Guid.NewGuid()));
        SetupVehicleReturns(new VehicleId(vehicleId), vehicle);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", vehicleId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage($"*{vehicleId}*not available*");
    }

    [Fact]
    public async Task Handle_WithSameVehicle_ShouldNotReassign()
    {
        var vehicleId = Guid.NewGuid();
        var driver = CreateOnDutyDriver(vehicleId);
        driver.ClearDomainEvents();
        SetupDriverReturns(driver);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", vehicleId);

        await _handler.Handle(command, CancellationToken.None);

        driver.CurrentVehicleId!.Value.Should().Be(vehicleId);
        driver.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullVehicle_WhenOnDutyDriver_ShouldUnassignBoth()
    {
        var vehicleId = Guid.NewGuid();
        var driver = CreateOnDutyDriver(vehicleId);
        SetupDriverReturns(driver);
        var vehicle = CreateActiveVehicle(driver.Id);
        SetupVehicleReturns(new VehicleId(vehicleId), vehicle);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", null);

        await _handler.Handle(command, CancellationToken.None);

        driver.CurrentVehicleId.Should().BeNull();
        driver.Status.Should().Be(DriverStatus.Available);
        vehicle.Status.Should().Be(VehicleStatus.Idle);
        vehicle.CurrentDriverId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNullVehicle_WhenAvailableDriver_ShouldNotCallUnassign()
    {
        var driver = CreateAvailableDriver();
        driver.ClearDomainEvents();
        SetupDriverReturns(driver);

        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123", null);

        await _handler.Handle(command, CancellationToken.None);

        driver.CurrentVehicleId.Should().BeNull();
        driver.Status.Should().Be(DriverStatus.Available);
        driver.DomainEvents.Should().BeEmpty();
    }
}
