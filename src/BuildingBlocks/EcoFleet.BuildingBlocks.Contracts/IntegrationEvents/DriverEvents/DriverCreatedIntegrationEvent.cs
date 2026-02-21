namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

public record DriverCreatedIntegrationEvent
{
    public Guid DriverId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public DateTime OccurredOn { get; init; }
}
