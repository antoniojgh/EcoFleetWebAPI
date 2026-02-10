using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Queries.GetAllDrivers
{
    public record GetAllDriversQuery : FilterDriverDTO, IRequest<PaginatedDTO<DriverDetailDTO>>;
}
