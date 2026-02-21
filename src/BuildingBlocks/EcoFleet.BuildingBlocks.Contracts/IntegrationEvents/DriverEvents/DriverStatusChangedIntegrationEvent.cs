namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

public record DriverStatusChangedIntegrationEvent
{
    public Guid DriverId { get; init; }
    public string NewStatus { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; }
}
