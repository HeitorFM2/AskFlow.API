# AskFlow.API

REST API backend for the **AskFlow** platform — a social/Q&A application supporting posts, threaded comments, likes, JWT authentication, and avatar upload. Built on **.NET 10** with Clean Architecture and CQRS.

---

## Overview

`AskFlow.API` is a .NET 10 Web API organized as **Clean Architecture** (Domain → Application → Infrastructure → WebAPI) with **CQRS** powered by MediatR. Every client request is dispatched to a `Command` (write) or `Query` (read) handler through a validation pipeline (FluentValidation), and every operation returns a typed `Result<T>` that maps cleanly to HTTP status codes.

Feature areas: **Auth** (register, login, refresh-token rotation with reuse detection, logout), **Posts** (CRUD + paginated listings, soft delete), **Comments** (threaded via `ParentCommentId`), **Likes** (toggle + liked-posts listing), **Users** (profile, avatar upload).

---

## Tech Stack

| Category | Technology |
|---|---|
| Runtime | **.NET 10** |
| Web | ASP.NET Core Web API |
| ORM | EF Core 10 (SQL Server) |
| Identity | ASP.NET Core Identity (`IdentityUser`) |
| Auth | JWT Bearer + rotating refresh tokens (SHA-256 hashed) |
| CQRS | **MediatR 14** |
| Validation | **FluentValidation 11** |
| Docs | Swashbuckle / OpenAPI |
| Storage | Azure Blob Storage (avatars) |
| Image processing | SixLabors.ImageSharp |
| Rate limiting | Built-in `Microsoft.AspNetCore.RateLimiting` |
| Health checks | `AddDbContextCheck` |
| Testing | xUnit + FluentAssertions + NSubstitute + Bogus (80% line coverage gate via coverlet) |
| CI/CD | GitHub Actions |

---

## Architecture

This project follows **Robert C. Martin's Clean Architecture** — dependencies point inward, the Domain depends on nothing.

![Clean Architecture](https://blog.cleancoder.com/uncle-bob/images/2012-08-13-the-clean-architecture/CleanArchitecture.jpg)

> Image © Robert C. Martin. Source: [The Clean Architecture — Clean Coder Blog (2012)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### Mapping to AskFlow.API

| Uncle Bob's layer | AskFlow project | What lives here |
|---|---|---|
| **Entities** (Enterprise Business Rules) | `AskFlow.Domain` | `User`, `Post`, `Comment`, `Like`, `RefreshToken` with encapsulated invariants. Zero infrastructure dependencies. |
| **Use Cases** (Application Business Rules) | `AskFlow.Application` | Commands, Queries, Handlers, Validators, `Result<T>`, `ErrorCodes` catalog, and interfaces for everything Infrastructure must provide. |
| **Interface Adapters** | `AskFlow.WebAPI` + `AskFlow.Infrastructure` | Controllers (thin — `Send()` to MediatR, convert `Result<T>` to `IActionResult`) and gateways (EF repositories, JWT generation, Blob storage adapter). |
| **Frameworks & Drivers** | EF Core, ASP.NET Core, Azure Blob, SQL Server, ImageSharp | All concrete external dependencies, injected via `Program.cs` and `AskFlow.Infrastructure/DependencyInjection.cs`. |

The `AskFlow.Tests` project (xUnit) covers all layers with an enforced 80% line-coverage threshold.

### Patterns in play

- **CQRS / MediatR** — every use case is `IRequest<TResponse>` + dedicated handler.
- **Pipeline behavior** — `ValidationBehavior<TRequest,TResponse>` runs FluentValidation before any handler executes.
- **Result Pattern** — no exceptions for flow control; `ResultType` (`Ok | NotFound | Unauthorized | Forbidden | Invalid | Conflict | Failure`) maps to HTTP status in a single place.
- **Repository + Read-Model split** — write repositories return entities; `*Queries` interfaces return flat ViewModels with optimized projections (e.g. `GetLikedPostIdsAsync` returns a `HashSet<int>` to mark `IsLiked` without N+1).
- **Unit of Work** — handlers never call `SaveChanges` directly.
- **Stable error codes** — every error response carries a machine-readable `code` (e.g. `POST_NOT_FOUND`, `AUTH_REFRESH_TOKEN_REUSE_DETECTED`) for i18n and client logic.

---

## Folder Structure

```
AskFlow.API/
├── AskFlow.API.slnx
├── .github/workflows/                  # CI/CD
├── AskFlow.Domain/
│   └── Entities/                       # User, Post, Comment, Like, RefreshToken
├── AskFlow.Application/
│   ├── DependencyInjection.cs
│   ├── Common/                         # Result, ErrorCodes, PagedResult, ValidationBehavior
│   ├── Interfaces/                     # 13 contracts for Infrastructure
│   ├── Auth/      { Commands, Handlers, ViewModels }
│   ├── Posts/     { Commands, Queries, Handlers, ViewModels }
│   ├── Comments/  { Commands, Queries, Handlers, ViewModels }
│   ├── Likes/     { Commands, Queries, Handlers, Dtos }
│   └── Users/     { Commands, Queries, Handlers, Dtos }
├── AskFlow.Infrastructure/
│   ├── DependencyInjection.cs
│   ├── Data/                           # AppDbContext + EF configurations
│   ├── Repositories/
│   ├── Services/                       # Token, Avatar, ImageProcessor, CurrentUser, ...
│   ├── Settings/
│   └── Migrations/
├── AskFlow.WebAPI/
│   ├── Program.cs
│   ├── Controllers/                    # Auth, Posts, Comments, Likes, Users
│   ├── Extensions/                     # GlobalExceptionHandler, ResultExtensions
│   └── appsettings.example*.json
└── AskFlow.Tests/                      # xUnit, 80% coverage gate
```

---

## Getting Started

### Prerequisites

- .NET SDK **10.0**
- SQL Server (Express, Developer, or container)
- (Optional) Azure Storage account — only needed to exercise avatar upload

### Setup

```bash
git clone https://github.com/HeitorFM2/AskFlow.API.git
cd AskFlow.API

# Copy config templates (real files are gitignored)
cp AskFlow.WebAPI/appsettings.example.json             AskFlow.WebAPI/appsettings.json
cp AskFlow.WebAPI/appsettings.example.Development.json AskFlow.WebAPI/appsettings.Development.json
```

Edit `appsettings.Development.json` and set your connection string and a JWT secret of **at least 32 UTF-8 bytes** (the app fails fast at startup otherwise).

```bash
# Apply migrations
dotnet tool install --global dotnet-ef
dotnet ef database update --project AskFlow.Infrastructure --startup-project AskFlow.WebAPI

# Run
dotnet run --project AskFlow.WebAPI
```

Swagger UI is served at the application root (`/`) in Development.

### Tests

```bash
dotnet test AskFlow.Tests/AskFlow.Tests.csproj --settings AskFlow.Tests/coverlet.runsettings
```

---

## API Reference

All endpoints are versioned under `/api/v2`. All require JWT except where noted; auth endpoints are rate-limited (10 req/min/IP), global limit 200 req/min/IP.

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v2/Auth/Register` | — | Create user, return tokens |
| `POST` | `/api/v2/Auth/Login` | — | Authenticate, return tokens |
| `POST` | `/api/v2/Auth/RefreshToken` | — | Rotate access + refresh token |
| `POST` | `/api/v2/Auth/Logout` | JWT | Revoke all refresh tokens |
| `GET`  | `/api/v2/Posts` | JWT | Paginated global feed (with `isLiked`) |
| `GET`  | `/api/v2/Posts/Me` | JWT | Posts by current user |
| `GET`  | `/api/v2/Posts/{id}/Details` | JWT | Post detail + comments |
| `POST` | `/api/v2/Posts` | JWT | Create post |
| `DELETE` | `/api/v2/Posts/{id}` | JWT (owner) | Soft delete |
| `GET`  | `/api/v2/Comments/{postId}` | JWT | Paginated comments |
| `GET`  | `/api/v2/Comments/{id}/Replies` | JWT | Threaded replies |
| `POST` | `/api/v2/Comments/{postId}` | JWT | Create comment (supports `parentCommentId`) |
| `DELETE` | `/api/v2/Comments/{id}` | JWT (owner) | Delete comment |
| `POST` | `/api/v2/Likes/{postId}` | JWT | Toggle like |
| `GET`  | `/api/v2/Likes/LikedPosts` | JWT | Posts liked by current user |
| `GET`  | `/api/v2/Users/Me` | JWT | Current user profile |
| `PATCH` | `/api/v2/Users/Avatar` | JWT | Upload avatar (multipart, ≤3 MB) |
| `GET`  | `/health` | — | Health check (app + DB) |

### Error format

```json
{ "code": "POST_NO_PERMISSION_TO_DELETE", "message": "You do not have permission to delete this post." }
```

Validation errors include an `errors[]` array with per-field `code`/`field`/`message`.

---

## Security

- **JWT** with full validation (issuer, audience, lifetime, signing key) and 30-second clock skew.
- **Refresh token rotation** — each refresh rotates the pair; detected reuse revokes **all** active tokens for that user.
- Refresh tokens stored as **SHA-256 hashes**, never in cleartext.
- **Identity lockout** — 5 failed attempts → 15-minute lock; strong password policy.
- **Rate limiting** — fixed-window per IP, dedicated stricter policy on `/api/v2/Auth/*`.
- **Avatar uploads** validated by `IImageProcessor` (rejects non-images) with a hard size cap.
- Resource ownership enforced in handlers (`Forbidden` on cross-user delete attempts).
- `GlobalExceptionHandler` returns sanitized error bodies with a `traceId` for correlation.

---

## Scalability & Operations

- **Stateless** — scales horizontally without sticky sessions; all state lives in SQL Server or Blob Storage.
- `EnableRetryOnFailure` on the EF provider handles transient cloud failures.
- `/health` is wired with `AddDbContextCheck<AppDbContext>` — Kubernetes/App Service ready.
- Mandatory pagination (`page`, `pageSize` validated, max 100) and `HashSet`-based `IsLiked` lookups eliminate N+1.
- Soft-delete preserves referential integrity.
- Structured logging via `ILogger<T>` throughout handlers; `ProblemDetails` registered for RFC 7807 clients.
- 80% line-coverage gate is enforced by coverlet on every `dotnet test`.

---

## Possible Improvements

- Distributed cache (Redis) for hot reads.
- Domain events / Outbox for side effects (notifications, search indexing).
- OpenTelemetry traces and metrics.
- API versioning via `Asp.Versioning` instead of path-only.
- Rate limiting partitioned per authenticated user.
- Secrets in Key Vault / user-secrets.

---

## License

No license file is currently declared in the repository — all rights reserved by the author (**Heitor F. M.**) until one is added.
