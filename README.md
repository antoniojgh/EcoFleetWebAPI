
## Project Overview

**EcoFleet** is a fleet management REST API built on **ASP.NET Core 10.0**. It manages vehicles, drivers, orders, managers, and driver-manager assignments.

- **Architecture:** Clean Architecture + Domain-Driven Design (DDD) + CQRS
- **Database:** SQL Server (LocalDB for dev), accessed via Entity Framework Core 10.0
- **Event publishing:** Transactional Outbox Pattern via MediatR
- **API documentation:** OpenAPI + Scalar UI (dev only)
- **Logging:** Serilog structured logging
- **Validation:** FluentValidation with MediatR pipeline behavior

---

## Solution Layout

```
EcoFleetWebAPI/
├── EcoFleet.API/                     # HTTP layer: controllers, middleware, request DTOs
├── EcoFleet.Application/             # Use cases: MediatR handlers, validators, DTOs
├── EcoFleet.Domain/                  # Core domain: entities, value objects, events
├── EcoFleet.Infrastructure/          # Data: EF Core, repositories, UoW, outbox processor
├── EcoFleet.Emails/                  # Email notifications (SMTP, event-driven)
├── EcoFleet.Application.UnitTests/   # Application layer tests
├── EcoFleet.Domain.UnitTests/        # Domain layer tests
└── EcoFleet.slnx                     # Solution file (modern Visual Studio format)
```

The solution enforces **strict layer boundaries**:

| Layer | Depends on |
|---|---|
| API | Application, Infrastructure, Emails |
| Application | Domain |
| Infrastructure | Application, Domain |
| Emails | Application |
| Domain | *(nothing)* |

---

## Running the Application

### Prerequisites
- .NET 10.0 SDK
- SQL Server LocalDB (ships with Visual Studio) or a full SQL Server instance

### Setup

```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update --project EcoFleet.Infrastructure --startup-project EcoFleet.API

# Run the API
dotnet run --project EcoFleet.API
```

Default URLs:
- HTTP: `http://localhost:5291`
- HTTPS: `https://localhost:7205`
- Scalar API docs (dev): `https://localhost:7205/scalar/v1`
- OpenAPI JSON: `https://localhost:7205/openapi/v1.json`

### Running Tests

```bash
# All tests
dotnet test

# Specific project
dotnet test EcoFleet.Application.UnitTests
dotnet test EcoFleet.Domain.UnitTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Adding a Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project EcoFleet.Infrastructure \
  --startup-project EcoFleet.API
```

---

## Configuration

Configuration is managed via `appsettings.json` (and `appsettings.Development.json`).

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Serilog` | Structured logging settings (console + file sinks) |
| `EMAIL_CONFIGURATIONS` | SMTP host/port/credentials for notifications |

**Secrets**: Email credentials and sensitive values should be overridden via [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "EMAIL_CONFIGURATIONS:EMAIL" "your@email.com" \
  --project EcoFleet.API
```

User Secrets ID: `792b7618-169d-408c-b652-7b4d4e5d6f32`

Log files are written to `C:\Logs\EcoFleet\` by default (Windows development path). Update `appsettings.json` for other environments.

---

## Architecture & Patterns

### CQRS with MediatR

All business operations are expressed as **commands** (write) or **queries** (read), dispatched through MediatR.

**Command example:**
```
EcoFleet.Application/UseCases/Vehicles/Commands/CreateVehicle/
├── CreateVehicleCommand.cs      # IRequest<Guid>
├── CreateVehicleHandler.cs      # IRequestHandler<CreateVehicleCommand, Guid>
└── CreateVehicleValidator.cs    # AbstractValidator<CreateVehicleCommand>
```

**Query example:**
```
EcoFleet.Application/UseCases/Vehicles/Queries/GetAllVehicle/
├── GetAllVehicleQuery.cs        # IRequest<PaginatedDTO<VehicleDTO>>
├── GetAllVehicleHandler.cs      # IRequestHandler<...>
└── GetAllVehicleValidator.cs
```

Controllers call `_mediator.Send(...)` only—never repositories directly.

### MediatR Pipeline Behaviors

Registered globally in `AddApplicationServices.cs`:

1. **`ValidationBehavior`** — Runs FluentValidation before every handler; throws `ValidationErrorException` on failure (HTTP 400).
2. **`LoggingBehavior`** — Logs request start/end and execution time; warns if request takes > 500 ms.

### Domain-Driven Design

**Aggregate Roots** (implement `IAggregateRoot`):
- `Vehicle` — Status: `Idle | Active | Maintenance`
- `Driver` — Status: `Available | OnDuty | Suspended`
- `Order` — Status: `Pending | InProgress | Completed | Cancelled`
- `Manager`
- `ManagerDriverAssignment`

**Value Objects** (immutable, equality by value, extend `ValueObject`):
- `LicensePlate`, `DriverLicense`, `Email`, `FullName`, `PhoneNumber`, `Geolocation`

**Domain Events** (implement `IDomainEvent`):
- `DriverSuspendedEvent`, `DriverReinstatedEvent`
- `VehicleDriverAssignedEvent`, `VehicleMaintenanceStartedEvent`

Business rules live in the **entity methods** (e.g., `driver.Suspend()` validates that the driver is not on duty). Never duplicate business logic in handlers.

### Transactional Outbox Pattern

Domain events are not published immediately. On `UnitOfWork.SaveChangesAsync()`:
1. Domain events are serialized to `OutboxMessages` rows in the **same database transaction**.
2. `OutboxProcessor` (a `BackgroundService`) polls every 60 seconds, processes up to 20 messages per batch.
3. It deserializes each message and publishes the event via `IPublisher` (MediatR).
4. Event handlers in `Application/UseCases/*/EventHandlers/` react (e.g., send emails).

This guarantees at-least-once delivery even if the process crashes mid-operation.

### Repository & Unit of Work

Repositories expose only what the domain needs (no generic `IQueryable` leakage). `IUnitOfWork` is the single commit point—always call it at the end of command handlers.

```csharp
// Correct handler pattern
var driver = await _repositoryDriver.FindAsync(id)
    ?? throw new NotFoundException(nameof(Driver), id);

driver.Suspend();
await _unitOfWork.SaveChangesAsync(cancellationToken); // commits + dispatches events
```

---

## API Endpoints

All routes are prefixed `/api/v1/`.

| Controller | Base Route | Key Actions |
|---|---|---|
| `VehiclesController` | `/api/v1/vehicles` | CRUD, mark for maintenance, telemetry |
| `DriversController` | `/api/v1/drivers` | CRUD, suspend, reinstate |
| `OrdersController` | `/api/v1/orders` | Create, start, complete, cancel |
| `ManagersController` | `/api/v1/managers` | CRUD |
| `ManagerDriverAssignmentsController` | `/api/v1/managerdriverassignments` | Create, activate, deactivate |

### Error Response Format

The `GlobalExceptionHandlerMiddleware` maps exceptions to HTTP status codes:

| Exception | HTTP Status |
|---|---|
| `ValidationErrorException` | 400 Bad Request |
| `NotFoundException` | 404 Not Found |
| `BusinessRuleException` | 409 Conflict |
| `DomainException` | 422 Unprocessable Entity |

---

## Conventions to Follow

### Adding a New Use Case

Follow this file structure (use an existing use case as a template):

```
EcoFleet.Application/UseCases/<Entity>/Commands/<ActionName>/
├── <ActionName>Command.cs
├── <ActionName>Handler.cs
└── <ActionName>Validator.cs
```

Steps:
1. Create the command/query record implementing `IRequest<T>`.
2. Create the handler implementing `IRequestHandler<TRequest, TResponse>`.
3. Create the validator extending `AbstractValidator<TCommand>`.
4. Add/extend the repository interface in `EcoFleet.Application/Interfaces/` if new data access is needed.
5. Implement the repository method in `EcoFleet.Infrastructure/Persistence/Repositories/`.
6. Wire the endpoint in the relevant controller in `EcoFleet.API/Controllers/`.
7. Write unit tests in `EcoFleet.Application.UnitTests/UseCases/<Entity>/Commands/`.

### Adding a New Domain Entity

1. Create the entity in `EcoFleet.Domain/Entities/` extending `Entity<TId>` and implementing `IAggregateRoot`.
2. Add value objects to `EcoFleet.Domain/ValueObjects/` extending `ValueObject`.
3. Add domain events to `EcoFleet.Domain/Events/` implementing `IDomainEvent`.
4. Add the `DbSet<T>` to `ApplicationDbContext`.
5. Create a Fluent API configuration class in `EcoFleet.Infrastructure/Persistence/Configurations/`.
6. Add a repository interface in `EcoFleet.Application/Interfaces/`.
7. Implement the repository in `EcoFleet.Infrastructure/Persistence/Repositories/`.
8. Register the repository in `AddInfrastructureServices.cs`.
9. Create an EF Core migration.

### Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Commands | `{Action}{Entity}Command` | `CreateVehicleCommand` |
| Queries | `Get{Entity}ByIdQuery` | `GetDriverByIdQuery` |
| Handlers | `{Action}{Entity}Handler` | `SuspendDriverHandler` |
| Validators | `{Action}{Entity}Validator` | `CreateOrderValidator` |
| Repositories | `IRepository{Entity}` / `Repository{Entity}` | `IRepositoryVehicle` |
| DTOs | `{Entity}DTO` | `VehicleDTO` |
| Domain Events | `{Entity}{Action}Event` | `DriverSuspendedEvent` |
| Configurations | `{Entity}Configuration` | `VehicleConfiguration` |

### Code Style

- **Nullable reference types** are enabled in all projects (`<Nullable>enable</Nullable>`). Annotate nullable returns and parameters explicitly.
- **Implicit usings** are enabled — no need to add common `System.*` or `Microsoft.*` usings manually.
- Business logic belongs in **domain entity methods**, not in handlers or repositories.
- Handlers should be thin: validate → load → mutate → save → return.
- Prefer **record types** for commands, queries, and DTOs.

### Testing Conventions

- **Framework:** xUnit v3 with NSubstitute (mocking) and FluentAssertions (assertions).
- Unit tests are in separate projects that mirror the source project structure.
- Mock all external dependencies (repositories, UnitOfWork, email service) with NSubstitute.
- Test both the happy path and failure scenarios (not found, business rule violations, validation failures).
- Domain unit tests test entity behavior directly (no mocks needed).
- Coverlet is configured for coverage reporting.

Example test class structure:
```csharp
public class SuspendDriverHandlerTests
{
    private readonly IRepositoryDriver _repositoryDriver = Substitute.For<IRepositoryDriver>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SuspendDriverHandler _handler;

    public SuspendDriverHandlerTests()
    {
        _handler = new SuspendDriverHandler(_repositoryDriver, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDriverExists_SuspendsDriver() { ... }

    [Fact]
    public async Task Handle_WhenDriverNotFound_ThrowsNotFoundException() { ... }
}
```

---

## Key Files Reference

| File | Purpose |
|---|---|
| `EcoFleet.API/Program.cs` | Application entry point, DI, middleware pipeline |
| `EcoFleet.API/Middleware/GlobalExceptionHandlerMiddleware.cs` | Centralized error handling |
| `EcoFleet.Application/AddApplicationServices.cs` | Application DI registration |
| `EcoFleet.Application/Behaviors/ValidationBehavior.cs` | Auto-validation pipeline |
| `EcoFleet.Application/Behaviors/LoggingBehavior.cs` | Request logging & perf monitoring |
| `EcoFleet.Infrastructure/AddInfrastructureServices.cs` | Infrastructure DI registration |
| `EcoFleet.Infrastructure/Persistence/ApplicationDbContext.cs` | EF Core DbContext |
| `EcoFleet.Infrastructure/Persistence/UnitOfWork.cs` | Commit + outbox event capture |
| `EcoFleet.Infrastructure/Outbox/OutboxProcessor.cs` | Background event publisher |
| `EcoFleet.Infrastructure/Migrations/` | EF Core migration history |
| `EcoFleet.Emails/AddEmailsServices.cs` | Email DI registration |
| `EcoFleet.Emails/NotificationsService.cs` | SMTP email dispatch |
| `appsettings.json` | Default configuration |

---

## What Does Not Exist (Yet)

- No CI/CD pipelines (no `.github/workflows/`)
- No Docker / docker-compose support
- No `.editorconfig` or StyleCop configuration
- No integration tests (only unit tests)
- No authentication/authorization layer
