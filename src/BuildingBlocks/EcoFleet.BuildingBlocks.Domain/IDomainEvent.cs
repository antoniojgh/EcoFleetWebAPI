using MediatR;

namespace EcoFleet.BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    DateTime OcurredOn { get; }
}
