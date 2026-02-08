using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Queries.GetAllVehicle
{
    public record GetAllVehicleQuery : FilterVehicleDTO, IRequest<PaginatedDTO<VehicleDetailDTO>>;
}
