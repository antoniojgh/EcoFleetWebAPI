using EcoFleet.Application.Exceptions;
using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Application.Interfaces.Emails;
using EcoFleet.Domain.Entities;
using EcoFleet.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverSuspended
{
    // 1. Implement INotificationHandler<T> for the specific event you want to consume
    public class DriverSuspendedEventHandler : INotificationHandler<DriverSuspendedEvent>
    {
        private readonly ILogger<DriverSuspendedEventHandler> _logger;
        private readonly INotificationsService _notificationsService;
        private readonly IRepositoryDriver _repository;

        public DriverSuspendedEventHandler(ILogger<DriverSuspendedEventHandler> logger, INotificationsService notificationsService,
                                           IRepositoryDriver repository)
        {
            _logger = logger;
            _notificationsService = notificationsService;
            _repository = repository;
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


            var driverId = new DriverId(notification.DriverId.Value);
            var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

            if (driver is null)
            {
                throw new NotFoundException(nameof(Driver), notification.DriverId.Value);
            }

            var driverDTO = DriverSuspendedEventDTO.FromEntity(driver);
            await _notificationsService.SendDriverSuspendedNotification(driverDTO);

            await Task.CompletedTask;
        }
    }
}
