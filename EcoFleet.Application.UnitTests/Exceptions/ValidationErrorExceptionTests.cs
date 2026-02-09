using EcoFleet.Application.Exceptions;
using FluentAssertions;
using FluentValidation.Results;

namespace EcoFleet.Application.UnitTests.Exceptions;

public class ValidationErrorExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        var failures = new List<ValidationFailure>
        {
            new("Field", "Error message")
        };

        var exception = new ValidationErrorException(failures);

        exception.Message.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public void Constructor_ShouldGroupErrorsByPropertyName()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required."),
            new("Name", "Name must be at least 3 characters."),
            new("Email", "Email is invalid.")
        };

        var exception = new ValidationErrorException(failures);

        exception.Errors.Should().HaveCount(2);
        exception.Errors["Name"].Should().HaveCount(2);
        exception.Errors["Name"].Should().Contain("Name is required.");
        exception.Errors["Name"].Should().Contain("Name must be at least 3 characters.");
        exception.Errors["Email"].Should().HaveCount(1);
        exception.Errors["Email"].Should().Contain("Email is invalid.");
    }

    [Fact]
    public void Constructor_WithNoFailures_ShouldHaveEmptyErrors()
    {
        var failures = new List<ValidationFailure>();

        var exception = new ValidationErrorException(failures);

        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithSingleFailure_ShouldHaveSingleEntry()
    {
        var failures = new List<ValidationFailure>
        {
            new("Field", "Error")
        };

        var exception = new ValidationErrorException(failures);

        exception.Errors.Should().HaveCount(1);
        exception.Errors["Field"].Should().ContainSingle().Which.Should().Be("Error");
    }
}
