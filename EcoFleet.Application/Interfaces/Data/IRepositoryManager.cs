using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IRepositoryManager : IRepository<Manager, ManagerId>
    {
        Task<IEnumerable<Manager>> GetFilteredAsync(FilterManagerDTO filterManagerDTO, CancellationToken cancellationToken);
    }
}
