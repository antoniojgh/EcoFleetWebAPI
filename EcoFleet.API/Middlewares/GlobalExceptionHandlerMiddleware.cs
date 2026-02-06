using System.Net;
using System.Text.Json;
using EcoFleet.Application.Exceptions; // Your custom ValidationException
using EcoFleet.Domain.Exceptions;     // Your custom DomainException

namespace EcoFleet.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            object response;

            switch (exception)
            {
                case ValidationErrorException validationEx:
                    // 400 Bad Request - Return the dictionary of validation errors
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        Type = "ValidationError",
                        Errors = validationEx.Errors
                    };
                    break;

                case NotFoundException notFoundEx:
                    // 404
                    statusCode = HttpStatusCode.NotFound; 
                    response = new
                    {
                        Type = "NotFound",
                        Message = notFoundEx.Message
                    };
                    break;

                case DomainException domainEx:
                    // 400 - Return the business rule violation message
                    statusCode = HttpStatusCode.BadRequest;
                    response = new
                    {
                        Type = "DomainError",
                        Message = domainEx.Message
                    };
                    break;

                default:
                    // 500 Internal Server Error - Hide implementation details
                    statusCode = HttpStatusCode.InternalServerError;
                    response = new
                    {
                        Type = "ServerError",
                        Message = "An unexpected error occurred."
                    };
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
