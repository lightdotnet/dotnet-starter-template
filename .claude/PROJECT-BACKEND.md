# Project — Backend

> Template. Populated incrementally, only when analysis is explicitly requested (see [workflows/analyze-solution.md](workflows/analyze-solution.md)). Do not fill this in speculatively. This is the backend half of [PROJECT.md](PROJECT.md) — see that file for the cross-cutting summary and for [PROJECT-CLIENTS.md](PROJECT-CLIENTS.md) (client apps).

## Backend Modules

> Modules are flat projects directly under `src/` (no `src/Modules/` nesting) — see [ARCHITECTURE-BACKEND.md § Module Structure Convention](ARCHITECTURE-BACKEND.md#backend--module-structure-convention). One entry per module actually built.

| Module | Path | Responsibility | Status |
|---|---|---|---|
| Identity | `src/Identity.Api/Identity.Api.csproj` + `src/Identity.Contracts/Identity.Contracts.csproj` | Users, roles, claims, JWT auth/token issuance. Single-project module (Entities/Data/Application/Services/Jwt/Controllers all in `Identity.Api`) + a `Contracts` seam project (DTOs, `IUserService`/`IRoleService`/`IServiceClaimService`). Kept the `.Api` name deliberately — anticipated candidate for future extraction into an independent identity service. | Built; internal layering is still informal and CQRS/service-class patterns coexist — see [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md). Covered by `tests/Identity.Tests` (98 tests, see the `Identity.Tests` row below). |
| Notifications | `src/Notifications.Api/Notifications.Api.csproj` + `src/Notifications.Contracts/Notifications.Contracts.csproj` | Notification storage + real-time push over SignalR. Single-project module, controllers split by *audience* rather than by resource: `NotificationController` (admin browse/send, permission-gated) vs. `UserNotificationController` (self-service, auto-scoped via `ICurrentUser`, no extra permission). Same `.Api`-suffix convention as `Identity`. See [docs/generated/backend/modules/Notifications.md](docs/generated/backend/modules/Notifications.md) for full detail. | Built. No automated test project yet — see [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md) for this and other open functional gaps. |

## Backend Key Projects

> Populated per-project as `analyze-project` runs are performed (e.g. shared/building-blocks, composition-root host). Do not assume completeness.

| Project | Path | Responsibility | Depends on |
|---|---|---|---|
| Shared | `src/Shared/Shared.csproj` | Shared kernel: base entity/DTO/value-object wrappers around the vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, permission-based authorization building blocks (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, `AuthorizationHandler`), FluentValidation pipeline behavior, constants. | (none — leaf project) |
| Infrastructure | `src/Infrastructure/Infrastructure.csproj` | Cross-cutting infrastructure: CORS, health checks, Serilog bootstrap logging (`AppLogging`), Mapster config, module/endpoint base classes (`AppModule`, `AppModuleEndpoint`), API controller base classes, Basic Auth attribute. EF Core/DbContext concerns live in `Persistence` (below), not here. | Shared |
| Persistence | `src/Persistence/Persistence.csproj` | EF Core provider configuration (`DbContextExtensions`/`DbProvider`, Sqlite `DateTimeOffset` workaround), DbContext base class (`Context/BaseDbContext`), audit/soft-delete tracking + domain-event dispatch for `DbContext.SaveChanges`, generic paging/result helpers, migration-time runtime support (`Migrations/` — design-time EF projects live separately under top-level `src/Migrations/`, excluded from this analysis pass). | Shared |
| Identity.Contracts | `src/Identity.Contracts/Identity.Contracts.csproj` | Public seam for the Identity module: DTOs, request types (incl. `SearchUserQuery`), `IUserService` (incl. `SearchAsync`)/`IRoleService`/`IServiceClaimService` (unimplemented — see [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md)). Not a true leaf — also references `Shared` (to reach a transitive vendor authorization package). | Shared |
| Identity.Api | `src/Identity.Api/Identity.Api.csproj` | The Identity module itself (see Backend Modules table above). | Identity.Contracts, Infrastructure, Persistence, Notifications.Contracts (welcome-email `IMailService` call from `UserCreatedEventHandler` — see [docs/generated/backend/modules/Identity.md](docs/generated/backend/modules/Identity.md)) |
| Notifications.Contracts | `src/Notifications.Contracts/Notifications.Contracts.csproj` | Public seam for the Notifications module: `NotificationDto`, `NotificationStatus` enum, `NotificationLookup` request type, push-message contracts (`SystemMessage`, `ForceLogoutMessage`), `INotificationService`, permission catalog (`NotificationPermissions`). True leaf. | Shared |
| Notifications.Api | `src/Notifications.Api/Notifications.Api.csproj` | The Notifications module itself (see Backend Modules table above). | Notifications.Contracts, Infrastructure, Persistence |
| StarterKit.WebApi | `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | Composition-root host (`Program.cs`) — the only executable/deployable project. | Identity.Api, Notifications.Api, Infrastructure, Shared |
| Framework.Tests | `tests/Framework.Tests/Framework.Tests.csproj` | xUnit v3 test project covering `Shared`, `Infrastructure`, `Persistence` only — does not reference `Identity.Api`/`Identity.Contracts` (that coverage lives in `Identity.Tests` instead). | Shared, Infrastructure, Persistence |
| Identity.Tests | `tests/Identity.Tests/Identity.Tests.csproj` | xUnit v3 + Moq test project covering the `Identity` module: `Extensions/`, `Jwt/` (incl. `internal` orchestration classes via `Identity.Api`'s `InternalsVisibleTo`), `Entities/`, `Services/`, `Controllers/`. Most service/Jwt tests run against a real Sqlite in-memory `IdentityDbContext` + ASP.NET Identity stack (`TestSupport/IdentityTestHost`) rather than mocking EF Core. 98 tests, all passing. | Identity.Api, Shared |

## Backend Cross-Cutting Concerns

> Only note items actually verified in code (e.g. shared logging, shared base entities, shared EF Core conventions). Do not guess.

- **Base entities**: `StarterKit.Entities.AuditableEntity`/`AuditableEntity<T>` and `DomainEvent` are thin wrappers around vendor `Light.Domain.Entities.*` types (`src/Shared/Entities/`) — modules should depend on these, not the vendor types directly. Note: `Identity`'s entities (`User`, `Role`, etc.) mostly can't use this directly since they must extend ASP.NET Identity's own base types (`IdentityUser`, `IdentityRole`) — a documented exception, not drift.
- **Current user abstraction**: `ICurrentUser` (`src/Shared/ICurrentUser.cs`) with `CurrentUserBase` (`src/Shared/Authorization/CurrentUserBase.cs`) deriving everything from a `ClaimsPrincipal`.
- **Permission-based authorization**: `SuperUserPolicy`/`AccessControl`/`AuthorizationHandler`/`PolicyProvider` (`src/Shared/Authorization/`), built on vendor `Light.AspNetCore.Authorization`.
- **Audit/soft-delete tracking**: `TrackingExtensions.AuditEntries` (`src/Persistence/Extensions/TrackingExtensions.cs`) — call before `SaveChanges` to stamp `Created`/`LastModified`/`*By` fields and apply soft-delete via `ISoftDelete`. `IdentityDbContext` calls this today but passes `enableSoftDelete: false` even though `User` implements `ISoftDelete` and `AuthenticationService` checks `Deleted != null` — see [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md) (D2).
- **Domain event dispatch**: `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Persistence/Extensions/`) publishes queued `BaseEvent`s via `Light.Mediator.IPublisher` and clears them — same "call it from `SaveChanges`" pattern as tracking. Not currently called from `IdentityDbContext`; Identity instead publishes its one domain event (`UserCreatedEvent`) manually from a CQRS command handler, bypassing this convention — see [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md) (P6).
- **DB provider abstraction**: `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext`/`ConfigureDatabase` (`src/Persistence/`) switch between InMemory/PostgreSQL/MSSQL/Sqlite based on an `IConfiguration["DbProvider"]` value — used today by `IdentityDbContext`.
- **Pagination**: `QueryableResultExtensions.ToPagedAsync` (`src/Persistence/Extensions/`) clamps `pageSize` to a max of 100 — a shared-kernel-level change affecting any current/future paginated endpoint, including `Identity`'s `UserService.SearchAsync`.
- **Bootstrap logging**: `AppLogging` (`src/Infrastructure/AppLogging.cs`) is a static Serilog logger (console + rolling file) for pre-host/startup logging (`Information`/`Warning` helpers), separate from per-request `ILogger<T>` DI.

## Backend Entry Points

- `src/StarterKit.WebApi/Program.cs` — the composition-root host, referencing `Identity.Api`, `Notifications.Api`, `Infrastructure`, `Shared`.

## Backend Open Questions / Gaps

See [docs/KNOWN_DEBT.md](docs/KNOWN_DEBT.md) for the current, maintained list of open backend debt/pending decisions.

---
_Last synced: 2026-08-13_
