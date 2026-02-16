using EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverSuspended;

namespace EcoFleet.Application.Interfaces.Emails
{
    public interface INotificationsService
    {
        Task SendDriverSuspendedNotification(DriverSuspendedEventDTO eventDTO);
    }
}
