using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IVehicleRepository
    {
        // We only expose what we strictly need for the logic
        Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(VehicleId id, CancellationToken cancellationToken = default);
        void Add(Vehicle vehicle);
    }
}
