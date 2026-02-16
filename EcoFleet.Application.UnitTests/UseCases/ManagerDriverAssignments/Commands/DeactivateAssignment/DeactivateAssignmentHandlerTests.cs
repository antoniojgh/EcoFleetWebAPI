using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.DeactivateAssignment;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Commands.DeactivateAssignment;

public class DeactivateAssignmentHandlerTests
{
    private readonly IRepositoryManagerDriverAssignment _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeactivateAssignmentHandler _handler;

    public DeactivateAssignmentHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManagerDriverAssignment>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeactivateAssignmentHandler(_repository, _unitOfWork);
    }

    private static ManagerDriverAssignment CreateActiveAssignment()
        => new(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()));

    private static ManagerDriverAssignment CreateInactiveAssignment()
    {
        var assignment = CreateActiveAssignment();
        assignment.Deactivate();
        return assignment;
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns((ManagerDriverAssignment?)null);

        var command = new DeactivateAssignmentCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns((ManagerDriverAssignment?)null);

        var command = new DeactivateAssignmentCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<ManagerDriverAssignment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAssignmentIsActive_ShouldDeactivate()
    {
        var assignment = CreateActiveAssignment();
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeactivateAssignmentCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        assignment.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenAssignmentIsActive_ShouldCallUpdateAndSaveChanges()
    {
        var assignment = CreateActiveAssignment();
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeactivateAssignmentCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(assignment, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenAssignmentIsAlreadyInactive_ShouldThrowDomainException()
    {
        var assignment = CreateInactiveAssignment();
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeactivateAssignmentCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Assignment is already inactive.");
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var assignmentGuid = Guid.NewGuid();
        var assignment = CreateActiveAssignment();
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var command = new DeactivateAssignmentCommand(assignmentGuid);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<ManagerDriverAssignmentId>(id => id.Value == assignmentGuid),
            Arg.Any<CancellationToken>());
    }
}
