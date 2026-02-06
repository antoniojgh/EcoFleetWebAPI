using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Application.Interfaces.Data
{
    public interface IRepositoryVehicle : IRepository<Vehicle, VehicleId>
    {
        Task<IEnumerable<Vehicle>> GetFilteredAsync(FilterVehicleDTO filterVehicleDTO);
    }
}
