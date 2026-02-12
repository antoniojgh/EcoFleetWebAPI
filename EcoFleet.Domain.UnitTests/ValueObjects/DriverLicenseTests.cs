using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class DriverLicenseTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnDriverLicense()
    {
        var license = DriverLicense.Create("dl-123");

        license.Should().NotBeNull();
        license.Value.Should().Be("DL-123");
    }

    [Fact]
    public void Create_ShouldConvertToUpperCase()
    {
        var license = DriverLicense.Create("abc-xyz");

        license.Value.Should().Be("ABC-XYZ");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var license = DriverLicense.Create("  DL-123  ");

        license.Value.Should().Be("DL-123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? value)
    {
        var act = () => DriverLicense.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage("Driver license cannot be empty.");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        var act = () => DriverLicense.Create("ABCDEFGHIJKLMNOPQRSTU");

        act.Should().Throw<DomainException>()
            .WithMessage("Driver license is too long.");
    }

    [Fact]
    public void Create_WithMaxLengthValue_ShouldSucceed()
    {
        var license = DriverLicense.Create("12345678901234567890");

        license.Value.Should().HaveLength(20);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var license1 = DriverLicense.Create("DL-123");
        var license2 = DriverLicense.Create("dl-123");

        license1.Should().Be(license2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var license1 = DriverLicense.Create("DL-123");
        var license2 = DriverLicense.Create("DL-456");

        license1.Should().NotBe(license2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeSame()
    {
        var license1 = DriverLicense.Create("DL-123");
        var license2 = DriverLicense.Create("dl-123");

        license1.GetHashCode().Should().Be(license2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ShouldDiffer()
    {
        var license1 = DriverLicense.Create("DL-123");
        var license2 = DriverLicense.Create("DL-456");

        license1.GetHashCode().Should().NotBe(license2.GetHashCode());
    }
}
