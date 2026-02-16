using EcoFleet.Application.UseCases.Managers.Queries.DTOs;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using FluentAssertions;

namespace EcoFleet.Application.UnitTests.UseCases.Managers.Queries.DTOs;

public class ManagerDetailDTOTests
{
    [Fact]
    public void FromEntity_ShouldMapAllFieldsCorrectly()
    {
        var manager = new Manager(
            FullName.Create("Alice", "Manager"),
            Email.Create("alice@example.com"));

        var dto = ManagerDetailDTO.FromEntity(manager);

        dto.Id.Should().Be(manager.Id.Value);
        dto.FirstName.Should().Be("Alice");
        dto.LastName.Should().Be("Manager");
        dto.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public void FromEntity_IdShouldMatchEntityId()
    {
        var manager = new Manager(
            FullName.Create("Alice", "Manager"),
            Email.Create("alice@example.com"));

        var dto = ManagerDetailDTO.FromEntity(manager);

        dto.Id.Should().Be(manager.Id.Value);
        dto.Id.Should().NotBe(Guid.Empty);
    }
}
