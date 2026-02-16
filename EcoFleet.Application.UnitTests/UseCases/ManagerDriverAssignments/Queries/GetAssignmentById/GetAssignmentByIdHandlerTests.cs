using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAssignmentById;
using EcoFleet.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Queries.GetAssignmentById;

public class GetAssignmentByIdHandlerTests
{
    private readonly IRepositoryManagerDriverAssignment _repository;
    private readonly GetAssignmentByIdHandler _handler;

    public GetAssignmentByIdHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManagerDriverAssignment>();
        _handler = new GetAssignmentByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns((ManagerDriverAssignment?)null);

        var query = new GetAssignmentByIdQuery(Guid.NewGuid());

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAssignmentExists_ShouldReturnCorrectDTO()
    {
        var managerId = new ManagerId(Guid.NewGuid());
        var driverId = new DriverId(Guid.NewGuid());
        var assignment = new ManagerDriverAssignment(managerId, driverId);
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var query = new GetAssignmentByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(assignment.Id.Value);
        result.ManagerId.Should().Be(managerId.Value);
        result.DriverId.Should().Be(driverId.Value);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenDeactivated_ShouldReturnInactiveDTO()
    {
        var assignment = new ManagerDriverAssignment(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()));
        assignment.Deactivate();
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var query = new GetAssignmentByIdQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        var assignmentGuid = Guid.NewGuid();
        var assignment = new ManagerDriverAssignment(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()));
        _repository.GetByIdAsync(Arg.Any<ManagerDriverAssignmentId>(), Arg.Any<CancellationToken>())
            .Returns(assignment);

        var query = new GetAssignmentByIdQuery(assignmentGuid);

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetByIdAsync(
            Arg.Is<ManagerDriverAssignmentId>(id => id.Value == assignmentGuid),
            Arg.Any<CancellationToken>());
    }
}
