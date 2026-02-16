using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmail()
    {
        var email = Email.Create("john@example.com");

        email.Should().NotBeNull();
        email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void Create_ShouldConvertToLowerCase()
    {
        var email = Email.Create("John@Example.COM");

        email.Value.Should().Be("john@example.com");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var email = Email.Create("  john@example.com  ");

        email.Value.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? value)
    {
        var act = () => Email.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    [InlineData("no-dot@domain")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        var longEmail = new string('a', 245) + "@example.com";

        var act = () => Email.Create(longEmail);

        act.Should().Throw<DomainException>()
            .WithMessage("Email is too long.");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var email1 = Email.Create("john@example.com");
        var email2 = Email.Create("JOHN@EXAMPLE.COM");

        email1.Should().Be(email2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var email1 = Email.Create("john@example.com");
        var email2 = Email.Create("jane@example.com");

        email1.Should().NotBe(email2);
    }

    [Fact]
    public void TryCreate_WithValidEmail_ShouldReturnEmail()
    {
        var email = Email.TryCreate("john@example.com");

        email.Should().NotBeNull();
        email!.Value.Should().Be("john@example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_WithNullOrWhitespace_ShouldReturnNull(string? value)
    {
        var email = Email.TryCreate(value);

        email.Should().BeNull();
    }

    [Fact]
    public void TryCreate_WithInvalidFormat_ShouldReturnNull()
    {
        var email = Email.TryCreate("not-an-email");

        email.Should().BeNull();
    }

    [Fact]
    public void TryCreate_WithTooLongValue_ShouldReturnNull()
    {
        var longEmail = new string('a', 245) + "@example.com";

        var email = Email.TryCreate(longEmail);

        email.Should().BeNull();
    }
}
