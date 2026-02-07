using EcoFleet.Application.UseCases.Vehicles.Queries.DTOs;
using MediatR;

namespace EcoFleet.Application.UseCases.Vehicles.Queries.GetVehicleById
{
    public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDetailDTO>;
}
