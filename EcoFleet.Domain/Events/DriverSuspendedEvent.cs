using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;

namespace EcoFleet.Domain.Events
{
    public record DriverSuspendedEvent(
        DriverId DriverId,
        DateTime OcurredOn
    ) : IDomainEvent;
}
