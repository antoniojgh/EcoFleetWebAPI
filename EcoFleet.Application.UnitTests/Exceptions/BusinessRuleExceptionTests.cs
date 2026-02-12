using EcoFleet.Application.Exceptions;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.Exceptions;

public class BusinessRuleExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        var exception = new BusinessRuleException("Driver is not available for assignment.");

        exception.Message.Should().Be("Driver is not available for assignment.");
    }

    [Fact]
    public void Constructor_WithInterpolatedMessage_ShouldFormatCorrectly()
    {
        var id = Guid.NewGuid();

        var exception = new BusinessRuleException($"Driver {id} is not available for assignment.");

        exception.Message.Should().Be($"Driver {id} is not available for assignment.");
    }

    [Fact]
    public void Constructor_ShouldBeOfTypeException()
    {
        var exception = new BusinessRuleException("Some rule violation.");

        exception.Should().BeAssignableTo<Exception>();
    }
}
