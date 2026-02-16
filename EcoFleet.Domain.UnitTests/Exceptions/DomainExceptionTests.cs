using EcoFleet.Domain.Exceptions;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        var exception = new DomainException("Something went wrong.");

        exception.Message.Should().Be("Something went wrong.");
    }

    [Fact]
    public void Constructor_ShouldBeOfTypeException()
    {
        var exception = new DomainException("Error");

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_ShouldPreserveExactMessage()
    {
        var message = "Cannot assign a vehicle to a suspended driver.";

        var exception = new DomainException(message);

        exception.Message.Should().Be(message);
    }
}
