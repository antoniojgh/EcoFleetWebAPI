using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class LicensePlateTests
{
    [Fact]
    public void Create_WithValidValue_ShouldReturnLicensePlate()
    {
        var plate = LicensePlate.Create("abc-123");

        plate.Should().NotBeNull();
        plate.Value.Should().Be("ABC-123");
    }

    [Fact]
    public void Create_ShouldConvertToUpperCase()
    {
        var plate = LicensePlate.Create("abc-xyz");

        plate.Value.Should().Be("ABC-XYZ");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldThrowDomainException(string? value)
    {
        var act = () => LicensePlate.Create(value!);

        act.Should().Throw<DomainException>()
            .WithMessage("License plate cannot be empty.");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldBeEqual()
    {
        var plate1 = LicensePlate.Create("ABC-123");
        var plate2 = LicensePlate.Create("abc-123");

        plate1.Should().Be(plate2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldNotBeEqual()
    {
        var plate1 = LicensePlate.Create("ABC-123");
        var plate2 = LicensePlate.Create("XYZ-789");

        plate1.Should().NotBe(plate2);
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeSame()
    {
        var plate1 = LicensePlate.Create("ABC-123");
        var plate2 = LicensePlate.Create("abc-123");

        plate1.GetHashCode().Should().Be(plate2.GetHashCode());
    }
}
