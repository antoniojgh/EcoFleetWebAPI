using EcoFleet.Application.UseCases.Managers.Queries.DTOs;
using MediatR;

namespace EcoFleet.Application.UseCases.Managers.Queries.GetManagerById
{
    public record GetManagerByIdQuery(Guid Id) : IRequest<ManagerDetailDTO>;
}
