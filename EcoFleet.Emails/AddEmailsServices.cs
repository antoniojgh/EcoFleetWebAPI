using EcoFleet.Application.Interfaces.Emails;
using EcoFleet.Emails.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.Emails
{
    public static class AddEmailsServices
    {
        public static IServiceCollection AddEmails(this IServiceCollection services)
        {
            services.AddScoped<INotificationsService, NotificationsService>();
            
            return services;
        }

    }
}
