using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Infrastructure.Outbox;
using EcoFleet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoFleet.Infrastructure
{
    public static class AddInfrastructureServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 1. Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Register Repositories
            services.AddScoped<IRepositoryVehicle, RepositoryVehicle>();
            services.AddScoped<IRepositoryDriver, RepositoryDriver>();
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IRepositoryManagerDriverAssignment, RepositoryManagerDriverAssignment>();
            services.AddScoped<IRepositoryOrder, RepositoryOrder>();

            // 3. Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 4. Register Outbox Processor (Background Worker)
            services.AddHostedService<OutboxProcessor>();

            return services;
        }
    }
}
