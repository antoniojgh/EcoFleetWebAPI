using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.UseCases.Managers.Queries.DTOs
{
    public record ManagerDetailDTO(
        Guid Id,
        string FirstName,
        string LastName,
        string Email
    )
    {
        public static ManagerDetailDTO FromEntity(Manager manager) =>
            new(
                manager.Id.Value,
                manager.Name.FirstName,
                manager.Name.LastName,
                manager.Email.Value
            );
    }
}
