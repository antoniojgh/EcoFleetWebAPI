namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

public record DriverSuspendedIntegrationEvent
{
    public Guid DriverId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; }
}
