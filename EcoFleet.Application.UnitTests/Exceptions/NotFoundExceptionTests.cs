using EcoFleet.Application.Exceptions;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_ShouldFormatMessageWithEntityNameAndKey()
    {
        var key = Guid.NewGuid();

        var exception = new NotFoundException("Vehicle", key);

        exception.Message.Should().Be($"Entity Vehicle with ID: {key} was not found.");
    }

    [Fact]
    public void Constructor_WithStringKey_ShouldFormatCorrectly()
    {
        var exception = new NotFoundException("Driver", "ABC-123");

        exception.Message.Should().Be("Entity Driver with ID: ABC-123 was not found.");
    }

    [Fact]
    public void Constructor_ShouldBeOfTypeException()
    {
        var exception = new NotFoundException("Vehicle", Guid.NewGuid());

        exception.Should().BeAssignableTo<Exception>();
    }
}
