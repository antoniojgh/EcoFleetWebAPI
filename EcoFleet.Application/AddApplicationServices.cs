using EcoFleet.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.Application;

public static class AddApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(AddApplicationServices).Assembly;

        // 1. Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            // Register the Validation Pipeline
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // 2. Register Validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}