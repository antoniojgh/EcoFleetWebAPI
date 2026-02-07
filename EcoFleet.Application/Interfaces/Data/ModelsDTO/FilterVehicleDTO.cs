using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.Interfaces.Data.ModelsDTO
{
    public class FilterVehicleDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsByPage { get; set; } = 10;
        public Guid? Id { get; set; }
        public string? Plate { get; set; }
        public VehicleStatus? Status { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid? CurrentDriverId { get; set; }
    }
}
