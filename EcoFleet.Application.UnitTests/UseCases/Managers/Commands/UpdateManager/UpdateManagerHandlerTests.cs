using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Commands.UpdateManager;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Commands.UpdateManager;

public class UpdateManagerHandlerTests
{
    private readonly IRepositoryManager _managerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateManagerHandler _handler;

    public UpdateManagerHandlerTests()
    {
        _managerRepository = Substitute.For<IRepositoryManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateManagerHandler(_managerRepository, _unitOfWork);
    }

    private static Manager CreateManager()
        => new(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com"));

    private void SetupManagerReturns(Manager manager)
    {
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);
    }

    [Fact]
    public async Task Handle_WhenManagerNotFound_ShouldThrowNotFoundException()
    {
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var command = new UpdateManagerCommand(Guid.NewGuid(), "Bob", "Smith", "bob@example.com");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenManagerNotFound_ShouldNotCallUpdate()
    {
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var command = new UpdateManagerCommand(Guid.NewGuid(), "Bob", "Smith", "bob@example.com");

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _managerRepository.DidNotReceive().Update(Arg.Any<Manager>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldUpdateNameAndEmail()
    {
        var manager = CreateManager();
        SetupManagerReturns(manager);

        var command = new UpdateManagerCommand(Guid.NewGuid(), "Bob", "Smith", "bob@example.com");

        await _handler.Handle(command, CancellationToken.None);

        manager.Name.FirstName.Should().Be("Bob");
        manager.Name.LastName.Should().Be("Smith");
        manager.Email.Value.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldCallUpdateAndSaveChanges()
    {
        var manager = CreateManager();
        SetupManagerReturns(manager);

        var command = new UpdateManagerCommand(Guid.NewGuid(), "Bob", "Smith", "bob@example.com");

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _managerRepository.Update(manager, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var managerGuid = Guid.NewGuid();
        var manager = CreateManager();
        SetupManagerReturns(manager);

        var command = new UpdateManagerCommand(managerGuid, "Bob", "Smith", "bob@example.com");

        await _handler.Handle(command, CancellationToken.None);

        await _managerRepository.Received(1).GetByIdAsync(
            Arg.Is<ManagerId>(id => id.Value == managerGuid),
            Arg.Any<CancellationToken>());
    }
}
