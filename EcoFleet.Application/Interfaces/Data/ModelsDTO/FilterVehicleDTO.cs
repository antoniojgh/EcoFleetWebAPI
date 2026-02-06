using EcoFleet.Domain.Enums;
using EcoFleet.Domain.ValueObjects;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data.ModelsDTO
{
    public class FilterVehicleDTO
    {
        public int Page { get; set; } = 1;
        public int RecordsByPage { get; set; } = 10;
        public VehicleId? Id { get; set; }
        public LicensePlate? Plate { get; set; }
        public VehicleStatus? Status { get; set; }
        public Geolocation? CurrentLocation { get; set; }
        public DriverId? CurrentDriverId { get; set; }
    }
}
