using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Queries.GetManagerById;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Queries.GetManagerById;

public class GetManagerByIdHandlerTests
{
    private readonly IRepositoryManager _repository;
    private readonly GetManagerByIdHandler _handler;

    public GetManagerByIdHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManager>();
        _handler = new GetManagerByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenManagerNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var query = new GetManagerByIdQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldReturnCorrectDTO()
    {
        var manager = new Manager(
            FullName.Create("Alice", "Manager"),
            Email.Create("alice@example.com"));
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);

        var query = new GetManagerByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(manager.Id.Value);
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Manager");
        result.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var managerGuid = Guid.NewGuid();
        var manager = new Manager(
            FullName.Create("Alice", "Manager"),
            Email.Create("alice@example.com"));
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);

        var query = new GetManagerByIdQuery(managerGuid);

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<ManagerId>(id => id.Value == managerGuid),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFoundMessage_ShouldContainEntityNameAndId()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var id = Guid.NewGuid();
        var query = new GetManagerByIdQuery(id);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Manager*{id}*");
    }
}
