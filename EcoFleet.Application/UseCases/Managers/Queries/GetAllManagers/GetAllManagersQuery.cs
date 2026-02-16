using EcoFleet.Application.Interfaces.Data.ModelsDTO;
using EcoFleet.Application.UseCases.Managers.Queries.DTOs;
using EcoFleet.Application.Utilities.Common;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Queries.GetAllManagers
{
    public record GetAllManagersQuery : FilterManagerDTO, IRequest<PaginatedDTO<ManagerDetailDTO>>;
}
