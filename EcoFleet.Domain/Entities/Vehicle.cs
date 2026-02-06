using EcoFleet.Domain.Common;
using EcoFleet.Domain.Enums;
using EcoFleet.Domain.Events;
using EcoFleet.Domain.Exceptions;
using EcoFleet.Domain.ValueObjects;

namespace EcoFleet.Domain.Entities
{
    public class Vehicle : Entity<VehicleId>, IAggregateRoot
    {
        public LicensePlate Plate { get; private set; }
        public VehicleStatus Status { get; private set; }
        public Geolocation CurrentLocation { get; private set; }

        public DriverId? CurrentDriverId { get; private set; }

        // Constructor for EF Core
        private Vehicle() : base(new VehicleId(Guid.NewGuid())) 
        { }

        // Public Constructor ensures valid initial state
        public Vehicle(LicensePlate plate, Geolocation initialLocation) : base(new VehicleId(Guid.NewGuid()))
        {
            Plate = plate;
            CurrentLocation = initialLocation;
            Status = VehicleStatus.Idle;
        }

        public Vehicle(LicensePlate plate, Geolocation initialLocation, DriverId initialDriverId ) : base(new VehicleId(Guid.NewGuid()))
        {
            Plate = plate;
            CurrentLocation = initialLocation;
            CurrentDriverId = initialDriverId;
            Status = VehicleStatus.Active;

            // Raise domain event for driver assignment
            AddDomainEvent(new VehicleDriverAssignedEvent(Id, initialDriverId, DateTime.UtcNow));
        }

        // --- BEHAVIORS (Methods, not Setters) ---

        public void UpdateTelemetry(Geolocation newLocation)
        {
            CurrentLocation = newLocation;
        }

        public void AssignDriver(DriverId driverId)
        {
            if (Status == VehicleStatus.Maintenance)
            {
                throw new DomainException("Cannot assign a driver to a vehicle in maintenance.");
            }

            CurrentDriverId = driverId;
            Status = VehicleStatus.Active;

            // Raise domain event for driver assignment
            AddDomainEvent(new VehicleDriverAssignedEvent(Id, driverId, DateTime.UtcNow));
        }

        public void MarkForMaintenance()
        {
            if (Status == VehicleStatus.Active)
            {
                throw new DomainException("Cannot maintain a vehicle currently in use.");
            }

            Status = VehicleStatus.Maintenance;
            CurrentDriverId = null;

            // Raise domain event for maintenance start
            AddDomainEvent(new VehicleMaintenanceStartedEvent(Id, DateTime.UtcNow));
        }
    }
}
