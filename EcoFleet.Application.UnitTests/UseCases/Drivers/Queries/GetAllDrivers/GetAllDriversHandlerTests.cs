using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Drivers.Queries.GetAllDrivers;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Queries.GetAllDrivers;

public class GetAllDriversHandlerTests
{
    private readonly IRepositoryDriver _repository;
    private readonly GetAllDriversHandler _handler;

    public GetAllDriversHandlerTests()
    {
        _repository = Substitute.For<IRepositoryDriver>();
        _handler = new GetAllDriversHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNoDrivers_ShouldReturnEmptyPaginatedResult()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterDriverDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Driver>());

        var query = new GetAllDriversQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenDriversExist_ShouldReturnCorrectCount()
    {
        var drivers = new List<Driver>
        {
            new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-111"), Email.Create("john@example.com")),
            new(FullName.Create("Jane", "Smith"), DriverLicense.Create("DL-222"), Email.Create("jane@example.com"))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterDriverDTO>(), Arg.Any<CancellationToken>())
            .Returns(drivers);

        var query = new GetAllDriversQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapDriversToDTOsCorrectly()
    {
        var vehicleId = Guid.NewGuid();
        var drivers = new List<Driver>
        {
            new(FullName.Create("John", "Doe"), DriverLicense.Create("DL-111"), Email.Create("john@example.com")),
            new(FullName.Create("Jane", "Smith"), DriverLicense.Create("DL-222"), Email.Create("jane@example.com"), new VehicleId(vehicleId))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterDriverDTO>(), Arg.Any<CancellationToken>())
            .Returns(drivers);

        var query = new GetAllDriversQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var items = result.Items.ToList();
        items[0].FirstName.Should().Be("John");
        items[0].LastName.Should().Be("Doe");
        items[0].License.Should().Be("DL-111");
        items[0].CurrentVehicleId.Should().BeNull();

        items[1].FirstName.Should().Be("Jane");
        items[1].LastName.Should().Be("Smith");
        items[1].CurrentVehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public async Task Handle_ShouldPassQueryAsFilterToRepository()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterDriverDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Driver>());

        var query = new GetAllDriversQuery();

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetFilteredAsync(query, Arg.Any<CancellationToken>());
    }
}
