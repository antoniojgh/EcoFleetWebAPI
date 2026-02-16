using EcoFleet.Domain.Common;
using EcoFleet.Domain.Exceptions;

namespace EcoFleet.Domain.Entities
{
    public class ManagerDriverAssignment : Entity<ManagerDriverAssignmentId>, IAggregateRoot
    {
        public ManagerId ManagerId { get; private set; }
        public DriverId DriverId { get; private set; }
        public DateTime AssignedDate { get; private set; }
        public bool IsActive { get; private set; }

        // Constructor for EF Core
        private ManagerDriverAssignment() : base(new ManagerDriverAssignmentId(Guid.NewGuid()))
        { }

        public ManagerDriverAssignment(ManagerId managerId, DriverId driverId)
            : base(new ManagerDriverAssignmentId(Guid.NewGuid()))
        {
            ManagerId = managerId;
            DriverId = driverId;
            AssignedDate = DateTime.UtcNow;
            IsActive = true;
        }

        // --- BEHAVIORS ---

        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException("Assignment is already inactive.");

            IsActive = false;
        }

        public void Activate()
        {
            if (IsActive)
                throw new DomainException("Assignment is already active.");

            IsActive = true;
        }
    }
}
