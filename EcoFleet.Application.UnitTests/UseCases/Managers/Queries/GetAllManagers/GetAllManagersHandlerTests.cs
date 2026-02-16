using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Managers.Queries.GetAllManagers;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Queries.GetAllManagers;

public class GetAllManagersHandlerTests
{
    private readonly IRepositoryManager _repository;
    private readonly GetAllManagersHandler _handler;

    public GetAllManagersHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManager>();
        _handler = new GetAllManagersHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNoManagers_ShouldReturnEmptyPaginatedResult()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Manager>());

        var query = new GetAllManagersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenManagersExist_ShouldReturnCorrectCount()
    {
        var managers = new List<Manager>
        {
            new(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com")),
            new(FullName.Create("Bob", "Smith"), Email.Create("bob@example.com"))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDTO>(), Arg.Any<CancellationToken>())
            .Returns(managers);

        var query = new GetAllManagersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapManagersToDTOsCorrectly()
    {
        var managers = new List<Manager>
        {
            new(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com")),
            new(FullName.Create("Bob", "Smith"), Email.Create("bob@example.com"))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDTO>(), Arg.Any<CancellationToken>())
            .Returns(managers);

        var query = new GetAllManagersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var items = result.Items.ToList();
        items[0].FirstName.Should().Be("Alice");
        items[0].LastName.Should().Be("Manager");
        items[0].Email.Should().Be("alice@example.com");

        items[1].FirstName.Should().Be("Bob");
        items[1].LastName.Should().Be("Smith");
        items[1].Email.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task Handle_ShouldPassQueryAsFilterToRepository()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Manager>());

        var query = new GetAllManagersQuery();

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetFilteredAsync(query, Arg.Any<CancellationToken>());
    }
}
