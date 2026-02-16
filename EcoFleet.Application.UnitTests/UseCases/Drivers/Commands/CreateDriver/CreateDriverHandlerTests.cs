using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Commands.CreateDriver;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Commands.CreateDriver;

public class CreateDriverHandlerTests
{
    private readonly IRepositoryDriver _driverRepository;
    private readonly IRepositoryVehicle _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateDriverHandler _handler;

    public CreateDriverHandlerTests()
    {
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _vehicleRepository = Substitute.For<IRepositoryVehicle>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateDriverHandler(_driverRepository, _vehicleRepository, _unitOfWork);
    }

    private Vehicle CreateIdleVehicle()
        => new(LicensePlate.Create("ABC-123"), Geolocation.Create(0, 0));

    private void SetupVehicleReturns(Guid vehicleId, Vehicle vehicle)
    {
        _vehicleRepository.GetByIdAsync(Arg.Is<VehicleId>(id => id.Value == vehicleId), Arg.Any<CancellationToken>())
            .Returns(vehicle);
    }

    [Fact]
    public async Task Handle_WithoutVehicle_ShouldReturnNonEmptyGuid()
    {
        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithoutVehicle_ShouldCallAddAsync()
    {
        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, null);

        await _handler.Handle(command, CancellationToken.None);

        await _driverRepository.Received(1).AddAsync(Arg.Is<Driver>(d =>
            d.Name.FirstName == "John" &&
            d.Name.LastName == "Doe" &&
            d.License.Value == "DL-123" &&
            d.Email.Value == "john@example.com" &&
            d.Status == DriverStatus.Available &&
            d.CurrentVehicleId == null), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WithoutVehicle_ShouldCallSaveChanges()
    {
        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, null);

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIdleVehicle_ShouldCreateOnDutyDriver()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicleId, vehicle);

        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, vehicleId);

        await _handler.Handle(command, CancellationToken.None);

        await _driverRepository.Received(1).AddAsync(Arg.Is<Driver>(d =>
            d.Status == DriverStatus.OnDuty &&
            d.CurrentVehicleId != null &&
            d.CurrentVehicleId.Value == vehicleId), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WithIdleVehicle_ShouldMarkVehicleActive()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = CreateIdleVehicle();
        SetupVehicleReturns(vehicleId, vehicle);

        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, vehicleId);

        await _handler.Handle(command, CancellationToken.None);

        vehicle.Status.Should().Be(VehicleStatus.Active);
        vehicle.CurrentDriverId.Should().NotBeNull();
        await _vehicleRepository.Received(1).Update(vehicle, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentVehicle_ShouldThrowNotFoundException()
    {
        var vehicleId = Guid.NewGuid();
        _vehicleRepository.GetByIdAsync(Arg.Any<VehicleId>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, vehicleId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithActiveVehicle_ShouldThrowBusinessRuleException()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0),
            new DriverId(Guid.NewGuid()));
        SetupVehicleReturns(vehicleId, vehicle);

        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, vehicleId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage($"*{vehicleId}*not available*");
    }

    [Fact]
    public async Task Handle_WithUnavailableVehicle_ShouldNotCreateDriver()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle(
            LicensePlate.Create("ABC-123"),
            Geolocation.Create(0, 0),
            new DriverId(Guid.NewGuid()));
        SetupVehicleReturns(vehicleId, vehicle);

        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, vehicleId);

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _driverRepository.DidNotReceive().AddAsync(Arg.Any<Driver>(), CancellationToken.None);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAddBeforeSaveChanges()
    {
        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, null);

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _driverRepository.AddAsync(Arg.Any<Driver>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_TwoInvocations_ShouldReturnDifferentIds()
    {
        var command = new CreateDriverCommand("John", "Doe", "DL-123", "john@example.com", null, null, null);

        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        result1.Should().NotBe(result2);
    }
}
