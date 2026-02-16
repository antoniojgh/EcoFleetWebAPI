using MediatR;

namespace EcoFleet.Application.UseCases.Orders.Commands.CreateOrder
{
    public record CreateOrderCommand(
        Guid DriverId,
        double PickUpLatitude,
        double PickUpLongitude,
        double DropOffLatitude,
        double DropOffLongitude,
        decimal Price
    ) : IRequest<Guid>;
}
