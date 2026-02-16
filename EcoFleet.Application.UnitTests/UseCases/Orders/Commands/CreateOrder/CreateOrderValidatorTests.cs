using EcoFleet.Application.UseCases.Orders.Commands.CreateOrder;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 40.416, -3.703, 41.385, 2.173, 25.50m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyDriverId_ShouldHaveError()
    {
        var command = new CreateOrderCommand(Guid.Empty, 40.416, -3.703, 41.385, 2.173, 25.50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DriverId)
            .WithErrorMessage("Driver Id is required.");
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WithInvalidPickUpLatitude_ShouldHaveError(double lat)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), lat, 0, 0, 0, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PickUpLatitude);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WithInvalidPickUpLongitude_ShouldHaveError(double lon)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 0, lon, 0, 0, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PickUpLongitude);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WithInvalidDropOffLatitude_ShouldHaveError(double lat)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 0, 0, lat, 0, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DropOffLatitude);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WithInvalidDropOffLongitude_ShouldHaveError(double lon)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 0, 0, 0, lon, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DropOffLongitude);
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveError()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 0, 0, 0, 0, -1m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price cannot be negative.");
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldHaveNoErrors()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), 0, 0, 0, 0, 0m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithBoundaryCoordinates_ShouldHaveNoErrors()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), -90, -180, 90, 180, 10m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
