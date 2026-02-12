using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Queries.GetDriverById;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Queries.GetDriverById;

public class GetDriverByIdHandlerTests
{
    private readonly IRepositoryDriver _repository;
    private readonly GetDriverByIdHandler _handler;

    public GetDriverByIdHandlerTests()
    {
        _repository = Substitute.For<IRepositoryDriver>();
        _handler = new GetDriverByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var query = new GetDriverByIdQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldReturnCorrectDTO()
    {
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var query = new GetDriverByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(driver.Id.Value);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.License.Should().Be("DL-123");
        result.Status.Should().Be(DriverStatus.Available);
        result.CurrentVehicleId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenDriverHasVehicle_ShouldReturnVehicleIdInDTO()
    {
        var vehicleId = Guid.NewGuid();
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"),
            new VehicleId(vehicleId));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var query = new GetDriverByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Status.Should().Be(DriverStatus.OnDuty);
        result.CurrentVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var driverGuid = Guid.NewGuid();
        var driver = new Driver(
            FullName.Create("John", "Doe"),
            DriverLicense.Create("DL-123"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var query = new GetDriverByIdQuery(driverGuid);

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<DriverId>(id => id.Value == driverGuid),
            Arg.Any<CancellationToken>());
    }
}
