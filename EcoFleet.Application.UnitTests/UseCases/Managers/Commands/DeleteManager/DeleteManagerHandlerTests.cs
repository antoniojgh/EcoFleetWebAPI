using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Managers.Commands.DeleteManager;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Commands.DeleteManager;

public class DeleteManagerHandlerTests
{
    private readonly IRepositoryManager _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteManagerHandler _handler;

    public DeleteManagerHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManager>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteManagerHandler(_repository, _unitOfWork);
    }

    private static Manager CreateManager()
        => new(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com"));

    [Fact]
    public async Task Handle_WhenManagerNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var command = new DeleteManagerCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenManagerNotFound_ShouldNotCallDelete()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var command = new DeleteManagerCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Delete(Arg.Any<Manager>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldCallDelete()
    {
        var manager = CreateManager();
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);

        var command = new DeleteManagerCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).Delete(manager, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldCallSaveChanges()
    {
        var manager = CreateManager();
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);

        var command = new DeleteManagerCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenManagerExists_ShouldCallDeleteBeforeSaveChanges()
    {
        var manager = CreateManager();
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(manager);

        var command = new DeleteManagerCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Delete(manager, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_NotFoundMessage_ShouldContainEntityNameAndId()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var id = Guid.NewGuid();
        var command = new DeleteManagerCommand(id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Manager*{id}*");
    }
}
