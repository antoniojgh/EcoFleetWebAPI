using EcoFleet.Application.UseCases.Drivers.Commands.UpdateDriver;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.Drivers.Commands.UpdateDriver;

public class UpdateDriverValidatorTests
{
    private readonly UpdateDriverValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", "DL-123456", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateDriverCommand(Guid.Empty, "John", "Doe", "DL-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Driver Id is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyFirstName_ShouldHaveError(string? firstName)
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), firstName!, "Doe", "DL-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyLastName_ShouldHaveError(string? lastName)
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", lastName!, "DL-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyLicense_ShouldHaveError(string? license)
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", license!, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.License)
            .WithErrorMessage("Driver license is required.");
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldHaveError()
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), new string('A', 101), "Doe", "DL-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is too long.");
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldHaveError()
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", new string('A', 101), "DL-123", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is too long.");
    }

    [Fact]
    public void Validate_WithTooLongLicense_ShouldHaveError()
    {
        var command = new UpdateDriverCommand(Guid.NewGuid(), "John", "Doe", new string('A', 21), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.License)
            .WithErrorMessage("Driver license is too long.");
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new UpdateDriverCommand(Guid.Empty, "", "", "", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.License);
    }
}
