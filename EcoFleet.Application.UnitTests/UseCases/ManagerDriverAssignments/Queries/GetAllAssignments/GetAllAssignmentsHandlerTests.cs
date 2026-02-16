using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Queries.GetAllAssignments;
using EcoFleet.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Queries.GetAllAssignments;

public class GetAllAssignmentsHandlerTests
{
    private readonly IRepositoryManagerDriverAssignment _repository;
    private readonly GetAllAssignmentsHandler _handler;

    public GetAllAssignmentsHandlerTests()
    {
        _repository = Substitute.For<IRepositoryManagerDriverAssignment>();
        _handler = new GetAllAssignmentsHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenNoAssignments_ShouldReturnEmptyPaginatedResult()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDriverAssignmentDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ManagerDriverAssignment>());

        var query = new GetAllAssignmentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenAssignmentsExist_ShouldReturnCorrectCount()
    {
        var assignments = new List<ManagerDriverAssignment>
        {
            new(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid())),
            new(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()))
        };
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDriverAssignmentDTO>(), Arg.Any<CancellationToken>())
            .Returns(assignments);

        var query = new GetAllAssignmentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldMapAssignmentsToDTOsCorrectly()
    {
        var managerId = new ManagerId(Guid.NewGuid());
        var driverId = new DriverId(Guid.NewGuid());
        var assignment = new ManagerDriverAssignment(managerId, driverId);

        var deactivatedAssignment = new ManagerDriverAssignment(new ManagerId(Guid.NewGuid()), new DriverId(Guid.NewGuid()));
        deactivatedAssignment.Deactivate();

        var assignments = new List<ManagerDriverAssignment> { assignment, deactivatedAssignment };
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDriverAssignmentDTO>(), Arg.Any<CancellationToken>())
            .Returns(assignments);

        var query = new GetAllAssignmentsQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        var items = result.Items.ToList();
        items[0].ManagerId.Should().Be(managerId.Value);
        items[0].DriverId.Should().Be(driverId.Value);
        items[0].IsActive.Should().BeTrue();

        items[1].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPassQueryAsFilterToRepository()
    {
        _repository.GetFilteredAsync(Arg.Any<FilterManagerDriverAssignmentDTO>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ManagerDriverAssignment>());

        var query = new GetAllAssignmentsQuery();

        await _handler.Handle(query, CancellationToken.None);

        await _repository.Received(1).GetFilteredAsync(query, Arg.Any<CancellationToken>());
    }
}
