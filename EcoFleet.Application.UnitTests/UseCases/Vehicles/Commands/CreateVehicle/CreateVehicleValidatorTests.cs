using EcoFleet.Application.UseCases.Vehicles.Commands.CreateVehicle;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.CreateVehicle;

public class CreateVehicleValidatorTests
{
    private readonly CreateVehicleValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidCommandAndDriver_ShouldHaveNoErrors()
    {
        var command = new CreateVehicleCommand("ABC-123", 40.416, -3.703, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyLicensePlate_ShouldHaveError(string? plate)
    {
        var command = new CreateVehicleCommand(plate!, 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LicensePlate)
            .WithErrorMessage("License plate is required.");
    }

    [Fact]
    public void Validate_WithTooLongLicensePlate_ShouldHaveError()
    {
        var command = new CreateVehicleCommand("ABCDEFGHIJKLMNOP", 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LicensePlate)
            .WithErrorMessage("License plate too long.");
    }

    [Fact]
    public void Validate_WithMaxLengthLicensePlate_ShouldHaveNoError()
    {
        var command = new CreateVehicleCommand("ABCDEFGHIJKLMNO", 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.LicensePlate);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-200)]
    [InlineData(200)]
    public void Validate_WithInvalidLatitude_ShouldHaveError(double lat)
    {
        var command = new CreateVehicleCommand("ABC-123", lat, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage("Invalid Latitude.");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-360)]
    [InlineData(360)]
    public void Validate_WithInvalidLongitude_ShouldHaveError(double lon)
    {
        var command = new CreateVehicleCommand("ABC-123", 0, lon, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage("Invalid Longitude.");
    }

    [Theory]
    [InlineData(90, 0)]
    [InlineData(-90, 0)]
    [InlineData(0, 180)]
    [InlineData(0, -180)]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    public void Validate_WithBoundaryCoordinates_ShouldHaveNoErrors(double lat, double lon)
    {
        var command = new CreateVehicleCommand("ABC-123", lat, lon, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldHaveMultipleErrors()
    {
        var command = new CreateVehicleCommand("", 200, 400, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LicensePlate);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
