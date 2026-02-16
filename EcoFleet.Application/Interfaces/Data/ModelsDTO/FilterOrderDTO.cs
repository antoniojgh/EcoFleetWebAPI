using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.Interfaces.Data.ModelsDTO
{
    public record FilterOrderDTO
    {
        public int Page { get; init; } = 1;
        public int RecordsByPage { get; init; } = 10;
        public Guid? Id { get; init; }
        public Guid? DriverId { get; init; }
        public OrderStatus? Status { get; init; }
    }
}
