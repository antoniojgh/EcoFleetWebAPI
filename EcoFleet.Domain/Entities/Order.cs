using EcoFleet.Domain.Common;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;

namespace EcoFleet.Domain.Entities
{
    public class Order : Entity<OrderId>, IAggregateRoot
    {
        public DriverId DriverId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? FinishedDate { get; private set; }
        public Geolocation PickUpLocation { get; private set; }
        public Geolocation DropOffLocation { get; private set; }
        public decimal Price { get; private set; }

        // Constructor for EF Core
        private Order() : base(new OrderId(Guid.NewGuid()))
        { }

        public Order(DriverId driverId, Geolocation pickUpLocation, Geolocation dropOffLocation, decimal price)
            : base(new OrderId(Guid.NewGuid()))
        {
            if (price < 0)
                throw new DomainException("Price cannot be negative.");

            DriverId = driverId;
            PickUpLocation = pickUpLocation;
            DropOffLocation = dropOffLocation;
            Price = price;
            Status = OrderStatus.Pending;
            CreatedDate = DateTime.UtcNow;
        }

        // --- BEHAVIORS ---

        public void StartProgress()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException("Only pending orders can be started.");

            Status = OrderStatus.InProgress;
        }

        public void Complete()
        {
            if (Status != OrderStatus.InProgress)
                throw new DomainException("Only in-progress orders can be completed.");

            Status = OrderStatus.Completed;
            FinishedDate = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
                throw new DomainException("Cannot cancel a completed order.");

            Status = OrderStatus.Cancelled;
            FinishedDate = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal price)
        {
            if (price < 0)
                throw new DomainException("Price cannot be negative.");

            Price = price;
        }
    }
}
