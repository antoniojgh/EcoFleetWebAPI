using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Enums;

namespace EcoFleet.Application.UseCases.Orders.Queries.DTOs
{
    public record OrderDetailDTO(
        Guid Id,
        Guid DriverId,
        OrderStatus Status,
        DateTime CreatedDate,
        DateTime? FinishedDate,
        double PickUpLatitude,
        double PickUpLongitude,
        double DropOffLatitude,
        double DropOffLongitude,
        decimal Price
    )
    {
        public static OrderDetailDTO FromEntity(Order order) =>
            new(
                order.Id.Value,
                order.DriverId.Value,
                order.Status,
                order.CreatedDate,
                order.FinishedDate,
                order.PickUpLocation.Latitude,
                order.PickUpLocation.Longitude,
                order.DropOffLocation.Latitude,
                order.DropOffLocation.Longitude,
                order.Price
            );
    }
}
