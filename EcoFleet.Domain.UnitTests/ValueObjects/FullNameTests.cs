using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class FullNameTests
{
    [Fact]
    public void Create_WithValidValues_ShouldReturnFullName()
    {
        var name = FullName.Create("John", "Doe");

        name.Should().NotBeNull();
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var name = FullName.Create("  John  ", "  Doe  ");

        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceFirstName_ShouldThrowDomainException(string? firstName)
    {
        var act = () => FullName.Create(firstName!, "Doe");

        act.Should().Throw<DomainException>()
            .WithMessage("First name cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceLastName_ShouldThrowDomainException(string? lastName)
    {
        var act = () => FullName.Create("John", lastName!);

        act.Should().Throw<DomainException>()
            .WithMessage("Last name cannot be empty.");
    }

    [Fact]
    public void Create_WithTooLongFirstName_ShouldThrowDomainException()
    {
        var longName = new string('A', 101);

        var act = () => FullName.Create(longName, "Doe");

        act.Should().Throw<DomainException>()
            .WithMessage("First name is too long.");
    }

    [Fact]
    public void Create_WithTooLongLastName_ShouldThrowDomainException()
    {
        var longName = new string('A', 101);

        var act = () => FullName.Create("John", longName);

        act.Should().Throw<DomainException>()
            .WithMessage("Last name is too long.");
    }

    [Fact]
    public void Create_WithMaxLengthNames_ShouldSucceed()
    {
        var maxName = new string('A', 100);

        var name = FullName.Create(maxName, maxName);

        name.FirstName.Should().HaveLength(100);
        name.LastName.Should().HaveLength(100);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var name1 = FullName.Create("John", "Doe");
        var name2 = FullName.Create("John", "Doe");

        name1.Should().Be(name2);
    }

    [Fact]
    public void Equals_WithDifferentFirstName_ShouldNotBeEqual()
    {
        var name1 = FullName.Create("John", "Doe");
        var name2 = FullName.Create("Jane", "Doe");

        name1.Should().NotBe(name2);
    }

    [Fact]
    public void Equals_WithDifferentLastName_ShouldNotBeEqual()
    {
        var name1 = FullName.Create("John", "Doe");
        var name2 = FullName.Create("John", "Smith");

        name1.Should().NotBe(name2);
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeSame()
    {
        var name1 = FullName.Create("John", "Doe");
        var name2 = FullName.Create("John", "Doe");

        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldDiffer()
    {
        var name1 = FullName.Create("John", "Doe");
        var name2 = FullName.Create("Jane", "Smith");

        name1.GetHashCode().Should().NotBe(name2.GetHashCode());
    }
}
