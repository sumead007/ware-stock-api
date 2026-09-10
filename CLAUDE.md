# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

WareStockApi is a .NET 10 solution generated from the [Clean.Architecture.Solution.Template](https://cleanarchitecture.jasontaylor.dev/docs/architecture/) (v10.8.0) by Jason Taylor, orchestrated with **.NET Aspire**. It currently still contains the template's demo domain (TodoLists/TodoItems) — this is the starting point to build the actual WareStock (inventory/warehouse) domain on top of.

## Commands

Run everything from the repo root (`WareStockApi.slnx`).

```bash
# Build the whole solution
dotnet build

# Run the app via Aspire AppHost (starts SQL Server container + Web API, opens Aspire dashboard)
dotnet run --project src/AppHost

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/Application.FunctionalTests
dotnet test tests/Application.UnitTests
dotnet test tests/Domain.UnitTests
dotnet test tests/Infrastructure.IntegrationTests

# Run a single test by name (NUnit filter)
dotnet test --filter "FullyQualifiedName~CreateTodoItemTests"

# Scaffold a new CQRS use case (run from src/Application)
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
# If the template isn't found:
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

`Application.FunctionalTests` spins up a real SQL Server via a nested Aspire host (`tests/TestAppHost`) using `DistributedApplicationTestingBuilder`, so **Docker must be running** for those tests (and for `dotnet run --project src/AppHost`). Tests reset the database between runs via `DatabaseResetter`/Respawn rather than recreating it per test.

`TreatWarningsAsErrors` is enabled solution-wide (`Directory.Build.props`) — a build with warnings will fail. Package versions are centrally managed in `Directory.Packages.props`; add new packages there, not with inline `Version=` attributes.

## Architecture

Standard Clean Architecture layering, enforced by project references (each layer only depends on the ones below it):

```
Domain          <- no dependencies
Application     <- depends on Domain
Infrastructure  <- depends on Application, Domain
Web             <- depends on Application, Infrastructure
AppHost         <- Aspire orchestrator, references Web (and ServiceDefaults)
```

- **Domain** (`src/Domain`): entities (`Entities/`), value objects (`ValueObjects/`), domain events (`Events/`), enums, and base types (`Common/BaseEntity`, `BaseAuditableEntity` for `Created/LastModified(By)` audit fields). Entities raise domain events via `BaseEntity.AddDomainEvent`.
- **Application** (`src/Application`): CQRS via MediatR. Each use case lives under `{Feature}/Commands/{UseCase}/` or `{Feature}/Queries/{UseCase}/` as a single file containing the `IRequest`/`IRequestHandler` pair plus a FluentValidation validator alongside it (e.g. `CreateTodoItemCommandValidator`). Cross-cutting concerns are MediatR pipeline behaviours registered in `DependencyInjection.cs` (`Common/Behaviours/`), applied in this order: `LoggingBehaviour` (pre-processor) → `UnhandledExceptionBehaviour` → `AuthorizationBehaviour` → `ValidationBehaviour` → `PerformanceBehaviour`. Authorization on a request is declared with `[Authorize(Roles = ..., Policy = ...)]` (`Common/Security/AuthorizeAttribute.cs`) and enforced by `AuthorizationBehaviour`, not in the Web layer. Persistence is accessed only through `IApplicationDbContext` (`Common/Interfaces`), never `ApplicationDbContext` directly, so handlers stay testable/infrastructure-agnostic. AutoMapper profiles and mapping-from-`IMapFrom<T>` conventions live near their DTOs.
- **Infrastructure** (`src/Infrastructure`): EF Core `ApplicationDbContext` (`Data/`), entity configurations (`Data/Configurations/`), `SaveChanges` interceptors for auditing and domain-event dispatch (`Data/Interceptors/`), and ASP.NET Core Identity (`Identity/`) implementing `IIdentityService`.
- **Web** (`src/Web`): ASP.NET Core Minimal API host. Endpoints are grouped by feature under `Endpoints/` as classes implementing `IEndpointGroup` (`Web/Infrastructure/IEndpointGroup.cs`) — `WebApplicationExtensions.MapEndpoints` reflects over the assembly, auto-registers every `IEndpointGroup` as a route group at `/api/{ClassName}` (override via static `RoutePrefix`) tagged for OpenAPI, and calls its static `Map`. Endpoints call `ISender.Send(command/query)` and return `TypedResults`; they contain no business logic. Route registration uses the custom `MapGet/MapPost/MapPut/MapPatch/MapDelete(Delegate, pattern)` overloads in `EndpointRouteBuilderExtensions.cs`, which derive the OpenAPI `operationId` from the handler method name — pattern is optional for GET/POST but required for PUT/PATCH/DELETE. OpenAPI docs are served via Scalar at `/scalar` (root `/` redirects there).
- **AppHost/ServiceDefaults/Shared** (`src/AppHost`, `src/ServiceDefaults`, `src/Shared`): Aspire orchestration. `AppHost/Program.cs` wires an Azure SQL Server container + `Web` project together for local `dotnet run`; `Shared/Services.cs` holds the shared resource-name constants (`WebApi`, `DatabaseServer`, `Database`) referenced by both `AppHost` and `TestAppHost`.

### Adding a new endpoint group
Create a class in `src/Web/Endpoints` implementing `IEndpointGroup`; it's picked up automatically — no manual registration in `Program.cs`.

### Adding a new use case
Scaffold with `dotnet new ca-usecase` (see Commands) or copy an existing sibling under `Application/{Feature}/Commands|Queries/`, then add/update the FluentValidation validator and wire a corresponding endpoint method in the feature's `IEndpointGroup`.
