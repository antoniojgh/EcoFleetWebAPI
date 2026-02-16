using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IRepositoryOrder : IRepository<Order, OrderId>
    {
        Task<IEnumerable<Order>> GetFilteredAsync(FilterOrderDTO filterOrderDTO, CancellationToken cancellationToken);
    }
}
