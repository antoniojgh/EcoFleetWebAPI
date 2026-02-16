using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IRepositoryManagerDriverAssignment : IRepository<ManagerDriverAssignment, ManagerDriverAssignmentId>
    {
        Task<IEnumerable<ManagerDriverAssignment>> GetFilteredAsync(FilterManagerDriverAssignmentDTO filterDTO, CancellationToken cancellationToken);
    }
}
