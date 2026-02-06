using MediatR;

namespace EcoFleet.Domain.Common
{
    // Represents something important that happened in the business.
    public interface IDomainEvent : INotification
    {
        DateTime OcurredOn { get; }
    }
}
