using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
using EcoFleet.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class RepositoryManagerDriverAssignment : Repository<ManagerDriverAssignment, ManagerDriverAssignmentId>, IRepositoryManagerDriverAssignment
    {
        private readonly ApplicationDbContext _context;

        public RepositoryManagerDriverAssignment(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ManagerDriverAssignment>> GetFilteredAsync(FilterManagerDriverAssignmentDTO filterDTO, CancellationToken cancellationToken = default)
        {
            var queryable = _context.ManagerDriverAssignments.AsQueryable();

            if (filterDTO.Id is not null)
            {
                var assignmentId = new ManagerDriverAssignmentId(filterDTO.Id.Value);
                queryable = queryable.Where(x => x.Id == assignmentId);
            }

            if (filterDTO.ManagerId is not null)
            {
                var managerId = new ManagerId(filterDTO.ManagerId.Value);
                queryable = queryable.Where(x => x.ManagerId == managerId);
            }

            if (filterDTO.DriverId is not null)
            {
                var driverId = new DriverId(filterDTO.DriverId.Value);
                queryable = queryable.Where(x => x.DriverId == driverId);
            }

            if (filterDTO.IsActive is not null)
            {
                queryable = queryable.Where(x => x.IsActive == filterDTO.IsActive);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterDTO.Page, filterDTO.RecordsByPage).ToListAsync(cancellationToken);
        }
    }
}
