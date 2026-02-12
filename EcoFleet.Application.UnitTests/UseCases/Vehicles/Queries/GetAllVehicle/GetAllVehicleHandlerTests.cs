using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Vehicles.Queries.GetAllVehicle;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Queries.GetAllVehicle;

public class GetAllVehicleHandlerTests
{
    private readonly IRepositoryVehicle _repository;
    private readonly GetAllVehicleHandler _handler;

    public GetAllVehicleHandlerTests()
    {
        _repository = Substitute.For<IRepositoryVehicle>();
        _handler = new GetAllVehicleHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNoVehicles_ShouldReturnEmptyPaginatedResult()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterVehicleDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Vehicle>());

        var query = new GetAllVehicleQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenVehiclesExist_ShouldReturnCorrectCount()
    {
        var vehicles = new List<Vehicle>
        {
            new(LicensePlate.Create("ABC-111"), Geolocation.Create(10, 20)),
            new(LicensePlate.Create("DEF-222"), Geolocation.Create(30, 40))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterVehicleDTO>(), Arg.Any<CancellationToken>())
            .Returns(vehicles);

        var query = new GetAllVehicleQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapVehiclesToDTOsCorrectly()
    {
        var driverId = Guid.NewGuid();
        var vehicles = new List<Vehicle>
        {
            new(LicensePlate.Create("ABC-111"), Geolocation.Create(10, 20)),
            new(LicensePlate.Create("XYZ-999"), Geolocation.Create(50, 60), new DriverId(driverId))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterVehicleDTO>(), Arg.Any<CancellationToken>())
            .Returns(vehicles);

        var query = new GetAllVehicleQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var items = result.Items.ToList();
        items[0].LicensePlate.Should().Be("ABC-111");
        items[0].Latitude.Should().Be(10);
        items[0].Longitude.Should().Be(20);
        items[0].CurrentDriverId.Should().BeNull();

        items[1].LicensePlate.Should().Be("XYZ-999");
        items[1].CurrentDriverId.Should().Be(driverId);
    }

    [Fact]
    public async Task Handle_ShouldPassQueryAsFilterToRepository()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterVehicleDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Vehicle>());

        var query = new GetAllVehicleQuery();

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetFilteredAsync(query, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TotalCount_ShouldReflectTotalNotFilteredCount()
    {
        var vehicles = new List<Vehicle>
        {
            new(LicensePlate.Create("ABC-111"), Geolocation.Create(10, 20))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterVehicleDTO>(), Arg.Any<CancellationToken>())
            .Returns(vehicles);

        var query = new GetAllVehicleQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}
