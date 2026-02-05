using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Domain.Events
{
    public record VehicleMaintenanceStartedEvent(

        VehicleId VehicleId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
