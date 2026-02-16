using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;
using EcoFleet.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure.Repositories
{
    public class RepositoryOrder : Repository<Order, OrderId>, IRepositoryOrder
    {
        private readonly ApplicationDbContext _context;

        public RepositoryOrder(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetFilteredAsync(FilterOrderDTO filterOrderDTO, CancellationToken cancellationToken = default)
        {
            var queryable = _context.Orders.AsQueryable();

            if (filterOrderDTO.Id is not null)
            {
                var orderId = new OrderId(filterOrderDTO.Id.Value);
                queryable = queryable.Where(x => x.Id == orderId);
            }

            if (filterOrderDTO.DriverId is not null)
            {
                var driverId = new DriverId(filterOrderDTO.DriverId.Value);
                queryable = queryable.Where(x => x.DriverId == driverId);
            }

            if (filterOrderDTO.Status is not null)
            {
                queryable = queryable.Where(x => x.Status == filterOrderDTO.Status);
            }

            return await queryable.OrderBy(x => x.Id).Paginate(filterOrderDTO.Page, filterOrderDTO.RecordsByPage).ToListAsync(cancellationToken);
        }
    }
}
