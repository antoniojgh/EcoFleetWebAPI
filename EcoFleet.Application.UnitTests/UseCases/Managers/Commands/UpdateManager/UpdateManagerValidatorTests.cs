using EcoFleet.Application.UseCases.Managers.Commands.UpdateManager;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Commands.UpdateManager;

public class UpdateManagerValidatorTests
{
    private readonly UpdateManagerValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", "Manager", "alice@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateManagerCommand(Guid.Empty, "Alice", "Manager", "alice@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Manager Id is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyFirstName_ShouldHaveError(string? firstName)
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), firstName!, "Manager", "alice@example.com");

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
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", lastName!, "alice@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", "Manager", email!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", "Manager", "not-an-email");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email format is invalid.");
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldHaveError()
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), new string('A', 101), "Manager", "alice@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is too long.");
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldHaveError()
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", new string('A', 101), "alice@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is too long.");
    }

    [Fact]
    public void Validate_WithTooLongEmail_ShouldHaveError()
    {
        var command = new UpdateManagerCommand(Guid.NewGuid(), "Alice", "Manager", new string('a', 245) + "@example.com");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is too long.");
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new UpdateManagerCommand(Guid.Empty, "", "", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
