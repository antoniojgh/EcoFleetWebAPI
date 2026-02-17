using System.Net;
using System.Net.Mail;
using EcoFleet.Application.Interfaces.Emails;
using EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverSuspended;
using EcoFleet.Application.UseCases.Drivers.EventHandlers.EventDriverReinstated;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EcoFleet.Emails.Notifications
{
    public class NotificationsService : INotificationsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationsService> _logger;

        public NotificationsService(IConfiguration configuration, ILogger<NotificationsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task SendDriverSuspendedNotification(DriverSuspendedEventDTO eventDTO)
        {
            var asunto = "Suspended driver";

            var cuerpo = $"""
        Dear {eventDTO.FirstName} {eventDTO.LastName}, 
            
        We inform you that your activity as a driver has been suspended.

        EcoFleet Team
        """;

            await SendMessage(eventDTO.Email, asunto, cuerpo);
        }

        public async Task SendDriverReinstatedNotification(DriverReinstatedEventDTO eventDTO)
        {
            var asunto = "Reinstated driver";

            var cuerpo = $"""
        Dear {eventDTO.FirstName} {eventDTO.LastName}, 
            
        We inform you that your activity as a driver has been reinstated.

        EcoFleet Team
        """;

            await SendMessage(eventDTO.Email, asunto, cuerpo);
        }

        private async Task SendMessage(string recipientEmail, string subject, string body)
        {
            _logger.LogInformation("Preparing to send email to {Recipient}. Subject: {Subject}", recipientEmail, subject);

            try
            {
                var ourEmail = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:EMAIL");
                var password = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:PASSWORD");
                var host = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:HOST");
                var port = _configuration.GetValue<int>("EMAIL_CONFIGURATIONS:PORT");

                var smtpCliente = new SmtpClient(host, port);
                smtpCliente.EnableSsl = true;
                smtpCliente.UseDefaultCredentials = false;
                smtpCliente.Credentials = new NetworkCredential(ourEmail, password);

                var mensaje = new MailMessage(ourEmail!, recipientEmail, subject, body);
                await smtpCliente.SendMailAsync(mensaje);

                _logger.LogInformation("Email sent successfully to {Recipient}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP Error: Failed to send email to {Recipient}", recipientEmail);

                // Rethrow so the Application layer knows it failed
                throw;
            }
        }
    }
}
