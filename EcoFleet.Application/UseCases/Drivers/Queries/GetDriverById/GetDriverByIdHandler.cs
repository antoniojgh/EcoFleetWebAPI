using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.UseCases.Drivers.Queries.DTOs;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Application.UseCases.Drivers.Queries.GetDriverById
{
    public class GetDriverByIdHandler : IRequestHandler<GetDriverByIdQuery, DriverDetailDTO>
    {
        private readonly IRepositoryDriver _repository;

        public GetDriverByIdHandler(IRepositoryDriver repository)
        {
            _repository = repository;
        }

        public async Task<DriverDetailDTO> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.Id);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), request.Id);
            }

            return DriverDetailDTO.FromEntity(driver);
        }
    }
}
