using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
        }

        public async Task<bool> ExistsAsync(VehicleId id, CancellationToken cancellationToken = default)
        {
            // Simple check without tracking for performance
            return await _context.Vehicles
                                 .AsNoTracking()
                                 .AnyAsync(v => v.Id == id, cancellationToken);
        }

        public async Task<Vehicle?> GetByIdAsync(VehicleId id, CancellationToken cancellationToken = default)
        {
            return await _context.Vehicles
                                 .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        }
    }
}
