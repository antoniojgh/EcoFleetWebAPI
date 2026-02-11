using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.CreateVehicle;

public class CreateVehicleHandlerTests
{
    private readonly IRepositoryVehicle _vehicleRepository;
    private readonly IRepositoryDriver _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateVehicleHandler _handler;

    public CreateVehicleHandlerTests()
    {
        _vehicleRepository = Substitute.For<IRepositoryVehicle>();
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateVehicleHandler(_vehicleRepository, _driverRepository, _unitOfWork);
    }

    private Driver CreateAvailableDriver(Guid driverId)
        => new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));

    private void SetupDriverReturns(Guid driverId, Driver driver)
    {
        _driverRepository.GetByIdAsync(Arg.Is<DriverId>(id => id.Value == driverId), Arg.Any<CancellationToken>())
            .Returns(driver);
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

        await _vehicleRepository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
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
    public async Task Handle_WithAvailableDriver_ShouldCreateActiveVehicle()
    {
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver(driverId);
        SetupDriverReturns(driverId, driver);

        var command = new CreateVehicleCommand("XYZ-789", 10.0, 20.0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        await _vehicleRepository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.Status == VehicleStatus.Active &&
            v.CurrentDriverId != null &&
            v.CurrentDriverId.Value == driverId));
    }

    [Fact]
    public async Task Handle_WithAvailableDriver_ShouldMarkDriverOnDuty()
    {
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver(driverId);
        SetupDriverReturns(driverId, driver);

        var command = new CreateVehicleCommand("XYZ-789", 10.0, 20.0, driverId);

        await _handler.Handle(command, CancellationToken.None);

        driver.Status.Should().Be(DriverStatus.OnDuty);
        driver.CurrentVehicleId.Should().NotBeNull();
        await _driverRepository.Received(1).Update(driver, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDriver_ShouldThrowNotFoundException()
    {
        var driverId = Guid.NewGuid();
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new CreateVehicleCommand("ABC-123", 0, 0, driverId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSuspendedDriver_ShouldThrowBusinessRuleException()
    {
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver(driverId);
        driver.Suspend();
        SetupDriverReturns(driverId, driver);

        var command = new CreateVehicleCommand("ABC-123", 0, 0, driverId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage($"*{driverId}*not available*");
    }

    [Fact]
    public async Task Handle_WithUnavailableDriver_ShouldNotCreateVehicle()
    {
        var driverId = Guid.NewGuid();
        var driver = CreateAvailableDriver(driverId);
        driver.Suspend();
        SetupDriverReturns(driverId, driver);

        var command = new CreateVehicleCommand("ABC-123", 0, 0, driverId);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _vehicleRepository.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAddBeforeSaveChanges()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _vehicleRepository.AddAsync(Arg.Any<Vehicle>());
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

        await _vehicleRepository.Received(1).AddAsync(Arg.Is<Vehicle>(v =>
            v.Plate.Value == "ABC-XYZ"));
    }
}
