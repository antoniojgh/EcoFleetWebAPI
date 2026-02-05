using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Domain.Events
{
    public record VehicleDriverAssignedEvent(

        VehicleId VehicleId,
        Guid DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
