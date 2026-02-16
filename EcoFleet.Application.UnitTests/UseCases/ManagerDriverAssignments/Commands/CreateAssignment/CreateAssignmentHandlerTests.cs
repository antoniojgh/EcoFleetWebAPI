using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Commands.CreateAssignment;

public class CreateAssignmentHandlerTests
{
    private readonly IRepositoryManagerDriverAssignment _assignmentRepository;
    private readonly IRepositoryManager _managerRepository;
    private readonly IRepositoryDriver _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateAssignmentHandler _handler;

    public CreateAssignmentHandlerTests()
    {
        _assignmentRepository = Substitute.For<IRepositoryManagerDriverAssignment>();
        _managerRepository = Substitute.For<IRepositoryManager>();
        _driverRepository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateAssignmentHandler(_assignmentRepository, _managerRepository, _driverRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidIds_ShouldReturnNonEmptyGuid()
    {
        var managerId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(new Manager(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com")));
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com")));

        var command = new CreateAssignmentCommand(managerId, driverId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithValidIds_ShouldCallAddAndSaveChanges()
    {
        var managerId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(new Manager(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com")));
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com")));

        var command = new CreateAssignmentCommand(managerId, driverId);

        await _handler.Handle(command, CancellationToken.None);

        await _assignmentRepository.Received(1).AddAsync(Arg.Any<ManagerDriverAssignment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentManager_ShouldThrowNotFoundException()
    {
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns((Manager?)null);

        var command = new CreateAssignmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentDriver_ShouldThrowNotFoundException()
    {
        _managerRepository.GetByIdAsync(Arg.Any<ManagerId>(), Arg.Any<CancellationToken>())
            .Returns(new Manager(FullName.Create("Alice", "Manager"), Email.Create("alice@example.com")));
        _driverRepository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new CreateAssignmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
