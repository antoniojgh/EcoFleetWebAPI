using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
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

        public async Task<IEnumerable<Vehicle>> GetFilteredAsync(FilterVehicleDTO filterVehicleDTO)
        {
            var queryable = _context.Vehicles.AsQueryable();

            if ((filterVehicleDTO.Id is not null))
            {
                queryable = queryable.Where(x => x.Id == filterVehicleDTO.Id);
            }

            if ((filterVehicleDTO.Plate is not null))
            {
                queryable = queryable.Where(x => x.Plate == filterVehicleDTO.Plate);
            }

            if ((filterVehicleDTO.Status is not null))
            {
                queryable = queryable.Where(x => x.Status == filterVehicleDTO.Status);
            }
            
            if ((filterVehicleDTO.CurrentLocation is not null))
            {
                queryable = queryable.Where(x => x.CurrentLocation == filterVehicleDTO.CurrentLocation);
            }
            
            if ((filterVehicleDTO.CurrentDriverId is not null))
            {
                queryable = queryable.Where(x => x.CurrentDriverId == filterVehicleDTO.CurrentDriverId);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterVehicleDTO.Page, filterVehicleDTO.RecordsByPage).ToListAsync();
        }
    }
}
