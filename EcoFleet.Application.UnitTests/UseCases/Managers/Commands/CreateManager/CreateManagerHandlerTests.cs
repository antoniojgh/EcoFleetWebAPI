using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Commands.CreateManager;
using EcoFleet.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Commands.CreateManager;

public class CreateManagerHandlerTests
{
    private readonly IRepositoryManager _managerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateManagerHandler _handler;

    public CreateManagerHandlerTests()
    {
        _managerRepository = Substitute.For<IRepositoryManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateManagerHandler(_managerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldReturnNonEmptyGuid()
    {
        var command = new CreateManagerCommand("Alice", "Manager", "alice@example.com");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsync()
    {
        var command = new CreateManagerCommand("Alice", "Manager", "alice@example.com");

        await _handler.Handle(command, CancellationToken.None);

        await _managerRepository.Received(1).AddAsync(Arg.Is<Manager>(m =>
            m.Name.FirstName == "Alice" &&
            m.Name.LastName == "Manager" &&
            m.Email.Value == "alice@example.com"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var command = new CreateManagerCommand("Alice", "Manager", "alice@example.com");

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallAddBeforeSaveChanges()
    {
        var command = new CreateManagerCommand("Alice", "Manager", "alice@example.com");

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _managerRepository.AddAsync(Arg.Any<Manager>(), Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_TwoInvocations_ShouldReturnDifferentIds()
    {
        var command = new CreateManagerCommand("Alice", "Manager", "alice@example.com");

        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        result1.Should().NotBe(result2);
    }
}
