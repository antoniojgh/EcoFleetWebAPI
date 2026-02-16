using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IRepositoryOrder _orderRepository;
        private readonly IRepositoryDriver _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderHandler(IRepositoryOrder orderRepository, IRepositoryDriver driverRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var driverId = new DriverId(request.DriverId);
            var driver = await _driverRepository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
                throw new NotFoundException(nameof(Driver), request.DriverId);

            var pickUpLocation = Geolocation.Create(request.PickUpLatitude, request.PickUpLongitude);
            var dropOffLocation = Geolocation.Create(request.DropOffLatitude, request.DropOffLongitude);

            var order = new Order(driverId, pickUpLocation, dropOffLocation, request.Price);

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return order.Id.Value;
        }
    }
}
