using EcoFleet.Application.UseCases.ManagerDriverAssignments.Commands.CreateAssignment;
using FluentValidation.TestHelper;

namespace EcoFleet.Application.UnitTests.UseCases.ManagerDriverAssignments.Commands.CreateAssignment;

public class CreateAssignmentValidatorTests
{
    private readonly CreateAssignmentValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateAssignmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyManagerId_ShouldHaveError()
    {
        var command = new CreateAssignmentCommand(Guid.Empty, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ManagerId)
            .WithErrorMessage("Manager Id is required.");
    }

    [Fact]
    public void Validate_WithEmptyDriverId_ShouldHaveError()
    {
        var command = new CreateAssignmentCommand(Guid.NewGuid(), Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DriverId)
            .WithErrorMessage("Driver Id is required.");
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var command = new CreateAssignmentCommand(Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ManagerId);
        result.ShouldHaveValidationErrorFor(x => x.DriverId);
    }
}
