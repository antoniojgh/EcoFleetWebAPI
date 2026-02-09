using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Domain.UnitTests.ValueObjects;

public class GeolocationTests
{
    [Fact]
    public void Create_WithValidCoordinates_ShouldReturnGeolocation()
    {
        var geo = Geolocation.Create(40.416, -3.703);

        geo.Should().NotBeNull();
        geo.Latitude.Should().Be(40.416);
        geo.Longitude.Should().Be(-3.703);
    }

    [Theory]
    [InlineData(-90, 0)]
    [InlineData(90, 0)]
    [InlineData(0, -180)]
    [InlineData(0, 180)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Create_WithBoundaryValues_ShouldSucceed(double lat, double lon)
    {
        var geo = Geolocation.Create(lat, lon);

        geo.Latitude.Should().Be(lat);
        geo.Longitude.Should().Be(lon);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-200)]
    [InlineData(200)]
    public void Create_WithInvalidLatitude_ShouldThrowDomainException(double lat)
    {
        var act = () => Geolocation.Create(lat, 0);

        act.Should().Throw<DomainException>()
            .WithMessage("Invalid Latitude.");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-360)]
    [InlineData(360)]
    public void Create_WithInvalidLongitude_ShouldThrowDomainException(double lon)
    {
        var act = () => Geolocation.Create(0, lon);

        act.Should().Throw<DomainException>()
            .WithMessage("Invalid Longitude.");
    }

    [Fact]
    public void Equals_WithSameCoordinates_ShouldBeEqual()
    {
        var geo1 = Geolocation.Create(40.416, -3.703);
        var geo2 = Geolocation.Create(40.416, -3.703);

        geo1.Should().Be(geo2);
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ShouldNotBeEqual()
    {
        var geo1 = Geolocation.Create(40.416, -3.703);
        var geo2 = Geolocation.Create(41.0, -3.703);

        geo1.Should().NotBe(geo2);
    }

    [Fact]
    public void GetHashCode_WithSameCoordinates_ShouldBeSame()
    {
        var geo1 = Geolocation.Create(40.416, -3.703);
        var geo2 = Geolocation.Create(40.416, -3.703);

        geo1.GetHashCode().Should().Be(geo2.GetHashCode());
    }
}
