namespace EcoFleet.Domain.Common
{
    // Represents something important that happened in the business.
    public interface IDomainEvent
    {
        DateTime OcurredOn { get; }
    }
}
