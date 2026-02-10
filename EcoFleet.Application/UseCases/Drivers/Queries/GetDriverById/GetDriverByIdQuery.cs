using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Queries.GetDriverById
{
    public record GetDriverByIdQuery(Guid Id) : IRequest<DriverDetailDTO>;
}
