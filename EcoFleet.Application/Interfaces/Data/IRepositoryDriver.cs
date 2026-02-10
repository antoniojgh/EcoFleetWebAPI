using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IRepositoryDriver : IRepository<Driver, DriverId>
    {
        Task<IEnumerable<Driver>> GetFilteredAsync(FilterDriverDTO filterDriverDTO, CancellationToken cancellationToken);
    }
}
