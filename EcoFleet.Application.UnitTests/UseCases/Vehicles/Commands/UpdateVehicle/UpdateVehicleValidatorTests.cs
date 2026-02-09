using EcoFleet.Application.UseCases.Vehicles.Commands.UpdateVehicle;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleValidatorTests
{
    private readonly UpdateVehicleValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 40.416, -3.703, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(Guid.Empty, "ABC-123", 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Vehicle Id is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyLicensePlate_ShouldHaveError(string? plate)
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), plate!, 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LicensePlate)
            .WithErrorMessage("License plate is required.");
    }

    [Fact]
    public void Validate_WithTooLongLicensePlate_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABCDEFGHIJKLMNOP", 0, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LicensePlate)
            .WithErrorMessage("License plate too long.");
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WithInvalidLatitude_ShouldHaveError(double lat)
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", lat, 0, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage("Invalid Latitude.");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WithInvalidLongitude_ShouldHaveError(double lon)
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", 0, lon, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage("Invalid Longitude.");
    }

    [Theory]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    public void Validate_WithBoundaryCoordinates_ShouldHaveNoErrors(double lat, double lon)
    {
        var command = new UpdateVehicleCommand(Guid.NewGuid(), "ABC-123", lat, lon, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new UpdateVehicleCommand(Guid.Empty, "", 200, 400, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.LicensePlate);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
