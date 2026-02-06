using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Domain.Events
{
    public record VehicleDriverAssignedEvent(

        VehicleId VehicleId,
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
