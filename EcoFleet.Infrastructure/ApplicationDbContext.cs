using EcoFleet.Domain.Entities;
using EcoFleet.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<ManagerDriverAssignment> ManagerDriverAssignments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Automatically apply all configurations (like VehicleConfiguration)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
