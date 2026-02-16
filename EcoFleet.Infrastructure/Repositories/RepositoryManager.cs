using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using EcoFleet.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class RepositoryManager : Repository<Manager, ManagerId>, IRepositoryManager
    {
        private readonly ApplicationDbContext _context;

        public RepositoryManager(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Manager>> GetFilteredAsync(FilterManagerDTO filterManagerDTO, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Managers.AsQueryable();

            if (filterManagerDTO.Id is not null)
            {
                var managerId = new ManagerId(filterManagerDTO.Id.Value);
                queryable = queryable.Where(x => x.Id == managerId);
            }

            if (filterManagerDTO.FirstName is not null)
            {
                queryable = queryable.Where(x => x.Name.FirstName.Contains(filterManagerDTO.FirstName));
            }

            if (filterManagerDTO.LastName is not null)
            {
                queryable = queryable.Where(x => x.Name.LastName.Contains(filterManagerDTO.LastName));
            }

            if (filterManagerDTO.Email is not null)
            {
                var email = Email.TryCreate(filterManagerDTO.Email);
                if (email is not null)
                    queryable = queryable.Where(x => x.Email == email);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterManagerDTO.Page, filterManagerDTO.RecordsByPage).ToListAsync(cancellationToken);
        }
    }
}
