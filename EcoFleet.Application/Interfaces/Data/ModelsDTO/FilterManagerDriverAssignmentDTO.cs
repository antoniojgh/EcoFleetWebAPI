namespace EcoFleet.Application.Interfaces.Data.ModelsDTO
{
    public record FilterManagerDriverAssignmentDTO
    {
        public int Page { get; init; } = 1;
        public int RecordsByPage { get; init; } = 10;
        public Guid? Id { get; init; }
        public Guid? ManagerId { get; init; }
        public Guid? DriverId { get; init; }
        public bool? IsActive { get; init; }
    }
}
