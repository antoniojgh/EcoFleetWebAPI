using EcoFleet.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoFleet.Application.UseCases.Drivers.EventHandlers
{
    // 1. Implement INotificationHandler<T> for the specific event you want to consume
    public class DriverSuspendedEventHandler : INotificationHandler<DriverSuspendedEvent>
    {
        private readonly ILogger<DriverSuspendedEventHandler> _logger;

        public DriverSuspendedEventHandler(ILogger<DriverSuspendedEventHandler> logger)
        {
            _logger = logger;
        }

        // 2. Implement the Handle method
        public async Task Handle(DriverSuspendedEvent notification, CancellationToken cancellationToken)
        {
            // logic: This is where you react to the event "after the fact"

            _logger.LogWarning(
                "DOMAINEVENT CONSUMED: Driver {DriverId} was suspended on {Date}. Notification sent to HR.",
                notification.DriverId.Value,
                notification.OcurredOn); //

            // Example real-world actions:
            // - Send an email via IEmailService
            // - Update a MongoDB Read Model for reporting
            // - Publish to RabbitMQ/AzureServiceBus (if integrating with external systems)

            await Task.CompletedTask;
        }
    }
}
