using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Create_WithValidPhoneNumber_ShouldReturnPhoneNumber()
    {
        var phone = PhoneNumber.Create("+1234567890");

        phone.Should().NotBeNull();
        phone.Value.Should().Be("+1234567890");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var phone = PhoneNumber.Create("  +1234567890  ");

        phone.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData("+1 234 567 890")]
    [InlineData("(123) 456-7890")]
    [InlineData("+44-20-7946-0958")]
    [InlineData("1234567")]
    public void Create_WithVariousFormats_ShouldSucceed(string value)
    {
        var phone = PhoneNumber.Create(value);

        phone.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? value)
    {
        var act = () => PhoneNumber.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage("Phone number cannot be empty.");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("phone")]
    [InlineData("123")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string value)
    {
        var act = () => PhoneNumber.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("Phone number format is invalid.");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+1234567890");

        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var phone1 = PhoneNumber.Create("+1234567890");
        var phone2 = PhoneNumber.Create("+0987654321");

        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void TryCreate_WithValidPhoneNumber_ShouldReturnPhoneNumber()
    {
        var phone = PhoneNumber.TryCreate("+1234567890");

        phone.Should().NotBeNull();
        phone!.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_WithNullOrWhitespace_ShouldReturnNull(string? value)
    {
        var phone = PhoneNumber.TryCreate(value);

        phone.Should().BeNull();
    }

    [Fact]
    public void TryCreate_WithInvalidFormat_ShouldReturnNull()
    {
        var phone = PhoneNumber.TryCreate("abc");

        phone.Should().BeNull();
    }
}
