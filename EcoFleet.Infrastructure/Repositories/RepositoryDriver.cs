using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using EcoFleet.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class RepositoryDriver : Repository<Driver, DriverId>, IRepositoryDriver
    {
        private readonly ApplicationDbContext _context;
        public RepositoryDriver(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Driver>> GetFilteredAsync(FilterDriverDTO filterDriverDTO, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Drivers.AsQueryable();

            if (filterDriverDTO.Id is not null)
            {
                var driverId = new DriverId(filterDriverDTO.Id.Value);
                queryable = queryable.Where(x => x.Id == driverId);
            }

            if (filterDriverDTO.FirstName is not null)
            {
                queryable = queryable.Where(x => x.Name.FirstName.Contains(filterDriverDTO.FirstName));
            }

            if (filterDriverDTO.LastName is not null)
            {
                queryable = queryable.Where(x => x.Name.LastName.Contains(filterDriverDTO.LastName));
            }

            if (filterDriverDTO.License is not null)
            {
                var license = DriverLicense.TryCreate(filterDriverDTO.License);
                if (license is not null)
                    queryable = queryable.Where(x => x.License == license);
            }

            if (filterDriverDTO.Status is not null)
            {
                queryable = queryable.Where(x => x.Status == filterDriverDTO.Status);
            }

            if (filterDriverDTO.CurrentVehicleId is not null)
            {
                var vehicleId = new VehicleId(filterDriverDTO.CurrentVehicleId.Value);
                queryable = queryable.Where(x => x.CurrentVehicleId == vehicleId);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterDriverDTO.Page, filterDriverDTO.RecordsByPage).ToListAsync(cancellationToken);
        }
    }
}
