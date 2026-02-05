using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;
using MediatR;

namespace EcoFleet.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPublisher _publisher; // MediatR Publisher

        public UnitOfWork(ApplicationDbContext dbContext, IPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Trigger Domain Events BEFORE committing to DB 
            // (Allows side effects to run or fail the request if needed)
            await PublishDomainEventsAsync(cancellationToken);

            // 2. Commit to Database
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
        {
            // Find all entities that have pending domain events
            var domainEntities = _dbContext.ChangeTracker
                .Entries<Entity<VehicleId>>() // Or use a generic base type if you have one shared
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            // If no events, exit
            if (!domainEntities.Any())
            {
                return;
            }

            // Collect all events into a single list
            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            // Clear the events from the entities (Prevent duplicate firing)
            domainEntities.ForEach(entity => entity.ClearDomainEvents());

            // Publish them to MediatR Handlers
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
