using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Commands.DeleteDriver;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Commands.DeleteDriver;

public class DeleteDriverHandlerTests
{
    private readonly IRepositoryDriver _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteDriverHandler _handler;

    public DeleteDriverHandlerTests()
    {
        _repository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteDriverHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new DeleteDriverCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldNotCallDelete()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new DeleteDriverCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Delete(Arg.Any<Driver>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldCallDelete()
    {
        var driver = new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new DeleteDriverCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).Delete(driver, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldCallSaveChanges()
    {
        var driver = new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new DeleteDriverCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDriverExists_ShouldCallDeleteBeforeSaveChanges()
    {
        var driver = new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"), Email.Create("john@example.com"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new DeleteDriverCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Delete(driver, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_NotFoundMessage_ShouldContainEntityNameAndId()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var id = Guid.NewGuid();
        var command = new DeleteDriverCommand(id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Driver*{id}*");
    }
}
