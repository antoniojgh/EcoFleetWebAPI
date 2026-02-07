using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using EcoFleet.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class RepositoryVehicle : Repository<Vehicle, VehicleId>, IRepositoryVehicle
    {
        private readonly ApplicationDbContext _context;
        public RepositoryVehicle(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetFilteredAsync(FilterVehicleDTO filterVehicleDTO, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Vehicles.AsQueryable();

            if (filterVehicleDTO.Id is not null)
            {
                var vehicleId = new VehicleId(filterVehicleDTO.Id.Value);
                queryable = queryable.Where(x => x.Id == vehicleId);
            }

            if (filterVehicleDTO.Plate is not null)
            {
                var plate = LicensePlate.Create(filterVehicleDTO.Plate);
                queryable = queryable.Where(x => x.Plate == plate);
            }

            if (filterVehicleDTO.Status is not null)
            {
                queryable = queryable.Where(x => x.Status == filterVehicleDTO.Status);
            }

            if (filterVehicleDTO.Latitude is not null)
            {
                queryable = queryable.Where(x => x.CurrentLocation.Latitude == filterVehicleDTO.Latitude);
            }

            if (filterVehicleDTO.Longitude is not null)
            {
                queryable = queryable.Where(x => x.CurrentLocation.Longitude == filterVehicleDTO.Longitude);
            }

            if (filterVehicleDTO.CurrentDriverId is not null)
            {
                var driverId = new DriverId(filterVehicleDTO.CurrentDriverId.Value);
                queryable = queryable.Where(x => x.CurrentDriverId == driverId);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterVehicleDTO.Page, filterVehicleDTO.RecordsByPage).ToListAsync(cancellationToken);
        }
    }
}
