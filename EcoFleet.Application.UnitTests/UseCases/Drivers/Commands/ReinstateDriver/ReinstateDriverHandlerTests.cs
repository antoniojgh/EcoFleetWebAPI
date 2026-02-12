using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Commands.ReinstateDriver;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Commands.ReinstateDriver;

public class ReinstateDriverHandlerTests
{
    private readonly IRepositoryDriver _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReinstateDriverHandler _handler;

    public ReinstateDriverHandlerTests()
    {
        _repository = Substitute.For<IRepositoryDriver>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new ReinstateDriverHandler(_repository, _unitOfWork);
    }

    private static Driver CreateSuspendedDriver()
    {
        var driver = new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));
        driver.Suspend();
        driver.ClearDomainEvents();
        return driver;
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new ReinstateDriverCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ShouldNotCallUpdate()
    {
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns((Driver?)null);

        var command = new ReinstateDriverCommand(Guid.NewGuid());

        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        await _repository.DidNotReceive().Update(Arg.Any<Driver>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDriverIsSuspended_ShouldReinstate()
    {
        var driver = CreateSuspendedDriver();
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new ReinstateDriverCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        driver.Status.Should().Be(DriverStatus.Available);
    }

    [Fact]
    public async Task Handle_WhenDriverIsSuspended_ShouldCallUpdateAndSaveChanges()
    {
        var driver = CreateSuspendedDriver();
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new ReinstateDriverCommand(Guid.NewGuid());

        await _handler.Handle(command, CancellationToken.None);

        Received.InOrder(() =>
        {
            _repository.Update(driver, Arg.Any<CancellationToken>());
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_WhenDriverIsAvailable_ShouldThrowDomainException()
    {
        var driver = new Driver(FullName.Create("John", "Doe"), DriverLicense.Create("DL-123"));
        _repository.GetByIdAsync(Arg.Any<DriverId>(), Arg.Any<CancellationToken>())
            .Returns(driver);

        var command = new ReinstateDriverCommand(Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Only suspended drivers can be reinstated.");
    }
}
