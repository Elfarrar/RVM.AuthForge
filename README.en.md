***English** | [Português](README.md)*

# RVM.AuthForge

Identity & Access Management platform built with ASP.NET Core, OpenIddict and PostgreSQL.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![OpenIddict](https://img.shields.io/badge/OpenIddict-7.4.0-blue)
![License](https://img.shields.io/badge/license-proprietary-red)

---

## About

**RVM.AuthForge** is a full-featured identity server that provides authentication, OAuth2/OIDC authorization, user management, API Keys and audit logging for the entire RVM Tech ecosystem. It includes an integrated Blazor Server administrative panel and supports a React SPA user portal as an OAuth2 client.

Key capabilities:
- OAuth2/OIDC server compatible with Authorization Code + PKCE, Client Credentials and Refresh Tokens
- API Key authentication with SHA256 hashing
- 2FA (Two-Factor Authentication) with TOTP and QR Code
- Complete audit log with IP, User-Agent and Correlation ID tracking
- Built-in Blazor Server administrative dashboard
- Per-endpoint rate limiting, health checks and configurable CORS

---

## Technologies

| Layer           | Technology                                         |
|-----------------|-----------------------------------------------------|
| Runtime         | .NET 10.0                                           |
| Web Framework   | ASP.NET Core 10                                     |
| Identity        | ASP.NET Core Identity                               |
| OAuth2/OIDC     | OpenIddict 7.4.0                                    |
| ORM             | Entity Framework Core 10                            |
| Database        | PostgreSQL (Npgsql 10.0.1)                           |
| Admin Dashboard | Blazor Server (Interactive Server Components)        |
| User Portal     | React SPA (port 5173)                                |
| Logging         | Serilog 10.0.0                                       |
| Health Checks   | AspNetCore.HealthChecks.NpgSql                       |
| Tests           | xUnit 2.9.3, Moq 4.20, EF Core InMemory, coverlet  |
| Test SDK        | Microsoft.NET.Test.Sdk 17.14.0                       |

---

## Architecture

```
+--------------------------------------------------+
|                   Clientes                       |
|   React SPA (5173)    Apps Externas    cURL/API  |
+--------+------------------+-------------+--------+
         |                  |             |
         | OAuth2/OIDC      | API Key     | Cookie
         |                  |             |
+--------v------------------v-------------v--------+
|              RVM.AuthForge.API                   |
|                                                  |
|  +-------------------+  +--------------------+   |
|  | Controllers       |  | Blazor Server      |   |
|  |  AccountController|  |  AdminDashboard    |   |
|  |  AdminController  |  |  UsersPage         |   |
|  |  Authorization    |  |  RolesPage         |   |
|  |   Controller      |  |  ApiKeysPage       |   |
|  +--------+----------+  |  ClientsPage       |   |
|           |              |  AuditLogPage      |   |
|  +--------v-----------+ +--------------------+   |
|  | Middleware          |                         |
|  |  CorrelationId      |  Auth                   |
|  |  RateLimiter        |  ApiKeyAuthHandler       |
|  +--------+------------+-----------+-------------+
|           |                        |
+-----------v------------------------v-------------+
|          RVM.AuthForge.Infrastructure            |
|                                                  |
|  +-------------------+  +--------------------+   |
|  | Services           |  | Data               |   |
|  |  ApiKeyService     |  |  AuthForgeDbContext |   |
|  |  AuditLogService   |  +--------------------+   |
|  +-------------------+                           |
|  +--------------------------------------------+  |
|  | DependencyInjection (Identity + OpenIddict)|  |
|  +--------------------------------------------+  |
+--------------------------------------------------+
|          RVM.AuthForge.Domain                    |
|                                                  |
|  Entities: ApplicationUser, ApplicationRole,     |
|            ApplicationApiKey, AuditLogEntry       |
|  Enums:    AuditAction                           |
+--------------------------------------------------+
               |
               v
        +-------------+
        | PostgreSQL   |
        +-------------+
```

---

## Project Structure

```
RVM.AuthForge/
|-- RVM.AuthForge.slnx
|-- global.json
|-- src/
|   |-- RVM.AuthForge.Domain/
|   |   |-- Entities/
|   |   |   |-- ApplicationUser.cs
|   |   |   |-- ApplicationRole.cs
|   |   |   |-- ApplicationApiKey.cs
|   |   |   |-- AuditLogEntry.cs
|   |   |-- Enums/
|   |   |   |-- AuditAction.cs
|   |   |-- RVM.AuthForge.Domain.csproj
|   |-- RVM.AuthForge.Infrastructure/
|   |   |-- Data/
|   |   |   |-- AuthForgeDbContext.cs
|   |   |-- Services/
|   |   |   |-- IApiKeyService.cs
|   |   |   |-- IAuditLogService.cs
|   |   |   |-- ApiKeyService.cs
|   |   |   |-- AuditLogService.cs
|   |   |-- DependencyInjection.cs
|   |   |-- RVM.AuthForge.Infrastructure.csproj
|   |-- RVM.AuthForge.API/
|       |-- Auth/
|       |   |-- ApiKeyAuthHandler.cs
|       |-- Controllers/
|       |   |-- AccountController.cs
|       |   |-- AdminController.cs
|       |   |-- AuthorizationController.cs
|       |-- Components/Pages/
|       |   |-- AdminDashboard.razor
|       |   |-- UsersPage.razor
|       |   |-- UserDetailPage.razor
|       |   |-- RolesPage.razor
|       |   |-- ApiKeysPage.razor
|       |   |-- ClientsPage.razor
|       |   |-- AuditLogPage.razor
|       |-- Middleware/
|       |   |-- CorrelationIdMiddleware.cs
|       |-- Services/
|       |   |-- SeedService.cs
|       |-- Program.cs
|       |-- RVM.AuthForge.API.csproj
|-- test/
    |-- RVM.AuthForge.Tests/
        |-- Helpers/
        |   |-- TestDbContext.cs
        |-- Services/
        |   |-- ApiKeyServiceTests.cs
        |   |-- AuditLogServiceTests.cs
        |-- RVM.AuthForge.Tests.csproj
```

---

## How to Run

### Prerequisites

- [.NET SDK 10.0.201+](https://dot.net/download)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Node.js 20+](https://nodejs.org/) (for the React Portal, optional)

### 1. Clone the repository

```bash
git clone https://github.com/rvmtech/RVM.AuthForge.git
cd RVM.AuthForge
```

### 2. Configure the connection string

Edit `src/RVM.AuthForge.API/appsettings.json` or set via environment variable:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=authforge;Username=postgres;Password=sua_senha"
  },
  "Admin": {
    "Email": "admin@authforge.dev",
    "Password": "Admin123!"
  }
}
```

### 3. Build and run

```bash
dotnet build
dotnet run --project src/RVM.AuthForge.API
```

The server will:
1. Automatically apply migrations to PostgreSQL
2. Create the `Admin` and `User` roles
3. Create the administrator user (as configured)
4. Register the pre-configured OAuth2 clients

### 4. Access

| Resource            | URL                            |
|---------------------|---------------------------------|
| API                 | `https://localhost:5001`        |
| Admin Dashboard     | `https://localhost:5001/admin`  |
| Health Check        | `https://localhost:5001/health` |
| React Portal (dev)  | `http://localhost:5173`         |

---

## API Endpoints

### Account (`/api/account`)

| Method | Endpoint                    | Description                        | Auth             | Rate Limit |
|--------|-----------------------------|------------------------------------|------------------|------------|
| POST   | `/api/account/register`     | Register a new user                | Public           | auth (10/min) |
| POST   | `/api/account/login`        | Login with email and password      | Public           | auth (10/min) |
| POST   | `/api/account/logout`       | Logout the authenticated user      | Bearer Token     | -          |
| GET    | `/api/account/profile`      | Get user profile                   | Bearer Token     | -          |
| PUT    | `/api/account/profile`      | Update name and avatar             | Bearer Token     | -          |
| POST   | `/api/account/change-password` | Change password                 | Bearer Token     | -          |
| POST   | `/api/account/forgot-password` | Request password reset          | Public           | auth (10/min) |
| POST   | `/api/account/reset-password`  | Reset password with token       | Public           | auth (10/min) |
| POST   | `/api/account/confirm-email`   | Confirm email with token        | Public           | -          |
| GET    | `/api/account/2fa/status`      | 2FA status                      | Bearer Token     | -          |
| POST   | `/api/account/2fa/enable`      | Generate TOTP key and QR Code URI | Bearer Token   | -          |
| POST   | `/api/account/2fa/verify`      | Verify code and activate 2FA   | Bearer Token     | -          |
| POST   | `/api/account/2fa/disable`     | Disable 2FA                     | Bearer Token     | -          |
| POST   | `/api/account/2fa/recovery-codes` | Generate new recovery codes | Bearer Token     | -          |

### Admin (`/api/admin`) -- Requires `Admin` role

| Method | Endpoint                          | Description                        |
|--------|-----------------------------------|------------------------------------|
| GET    | `/api/admin/users`                | List users (paginated, searchable) |
| GET    | `/api/admin/users/{id}`           | User details                       |
| PUT    | `/api/admin/users/{id}`           | Update user name and status        |
| POST   | `/api/admin/users/{id}/roles`     | Assign role to a user              |
| DELETE | `/api/admin/users/{id}/roles/{role}` | Remove role from a user         |
| POST   | `/api/admin/users/{id}/lock`      | Lock user account                  |
| POST   | `/api/admin/users/{id}/unlock`    | Unlock user account                |
| GET    | `/api/admin/roles`                | List all roles                     |
| POST   | `/api/admin/roles`                | Create new role                    |
| DELETE | `/api/admin/roles/{id}`           | Delete role                        |
| GET    | `/api/admin/api-keys`             | List API Keys                      |
| POST   | `/api/admin/api-keys`             | Create new API Key                 |
| POST   | `/api/admin/api-keys/{id}/revoke` | Revoke API Key                     |
| GET    | `/api/admin/clients`              | List OAuth clients                 |
| POST   | `/api/admin/clients`              | Create OAuth client                |
| PUT    | `/api/admin/clients/{id}`         | Update OAuth client                |
| GET    | `/api/admin/audit`                | Query audit log (with filters)     |

### OAuth2/OIDC (OpenIddict)

| Method   | Endpoint              | Description                       |
|----------|-----------------------|-----------------------------------|
| GET/POST | `/connect/authorize`  | Authorization endpoint            |
| POST     | `/connect/token`      | Token issuance                    |
| GET      | `/connect/userinfo`   | Authenticated user information    |
| GET/POST | `/connect/logout`     | End session / logout              |

### Infrastructure

| Method | Endpoint    | Description              |
|--------|-------------|--------------------------|
| GET    | `/health`   | Health check (PostgreSQL)|

---

## Blazor Admin Dashboard

The administrative panel is served as interactive Blazor Server, accessible at `/admin`. Available pages:

| Route                   | Page                  | Description                                     |
|-------------------------|-----------------------|-------------------------------------------------|
| `/admin`                | Dashboard             | Metrics: total users, active, 2FA, events in 24h. Recent activity table. |
| `/admin/users`          | Users                 | Paginated listing with search, account lock/unlock. |
| `/admin/users/{id}`     | User Detail           | Full info, role management, audit history.      |
| `/admin/roles`          | Roles                 | Create, list and delete roles.                  |
| `/admin/api-keys`       | API Keys              | Create, list and revoke API Keys. Key shown only at creation. |
| `/admin/clients`        | OAuth Clients         | Create and list OAuth2 clients (public and confidential). |
| `/admin/audit`          | Audit Log             | Query with filters by action, user, date. Paginated. |

---

## React Portal

The user portal is a React SPA (public OAuth2 client) running at `http://localhost:5173`. Planned pages:

| Page         | Description                                         |
|--------------|-----------------------------------------------------|
| Login        | Redirect to the Authorization Server via OAuth2 PKCE |
| Callback     | Receives the authorization code and exchanges it for tokens |
| Profile      | View and edit profile (name, avatar)                |
| Security     | Change password, enable/disable 2FA, recovery codes |
| Logout       | End session via OIDC                                |

**CORS** configured for `http://localhost:5173` with credentials enabled.

---

## OAuth2/OIDC

### Supported Flows

| Flow                         | Description                                     |
|------------------------------|-------------------------------------------------|
| Authorization Code + PKCE    | For SPAs and public applications. PKCE required. |
| Client Credentials           | For machine-to-machine (M2M) communication.      |
| Refresh Token                | Access token renewal without re-authentication.  |

### Token Configuration

| Property               | Value           |
|------------------------|-----------------|
| Access Token Lifetime  | 30 minutes      |
| Refresh Token Lifetime | 7 days          |
| AT Encryption          | Disabled        |
| Registered Scopes      | `openid`, `profile`, `email`, `api` |

### Pre-Configured Clients (Seed)

#### authforge-portal (Public)

| Property          | Value                                  |
|-------------------|----------------------------------------|
| Client ID         | `authforge-portal`                     |
| Type              | Public                                 |
| Display Name      | RVM.AuthForge Portal (React SPA)       |
| Redirect URI      | `http://localhost:5173/callback`        |
| Post-Logout URI   | `http://localhost:5173/`                |
| Grant Types       | Authorization Code, Refresh Token      |
| PKCE              | Required                               |
| Scopes            | email, profile, roles, api             |

#### demo-api (Confidential)

| Property          | Value                                  |
|-------------------|----------------------------------------|
| Client ID         | `demo-api`                             |
| Client Secret     | `demo-api-secret`                      |
| Type              | Confidential                           |
| Display Name      | Demo API (Confidential)                |
| Grant Types       | Client Credentials                     |
| Scopes            | api                                    |

### Example: Authorization Code + PKCE

```
GET /connect/authorize?
  client_id=authforge-portal&
  redirect_uri=http://localhost:5173/callback&
  response_type=code&
  scope=openid profile email api&
  code_challenge=<SHA256_HASH>&
  code_challenge_method=S256&
  state=<RANDOM>
```

```
POST /connect/token
  grant_type=authorization_code&
  client_id=authforge-portal&
  code=<AUTH_CODE>&
  redirect_uri=http://localhost:5173/callback&
  code_verifier=<ORIGINAL_VERIFIER>
```

### Example: Client Credentials

```
POST /connect/token
  grant_type=client_credentials&
  client_id=demo-api&
  client_secret=demo-api-secret&
  scope=api
```

---

## Tests

The project uses **xUnit** with **Moq** and **EF Core InMemory** for unit tests.

```bash
# Run all tests
dotnet test

# Run with coverage (coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test
dotnet test --filter "FullyQualifiedName~ApiKeyServiceTests"
```

### Existing Tests

| Class                   | Tests  | Coverage                                                 |
|-------------------------|--------|----------------------------------------------------------|
| `ApiKeyServiceTests`    | 7      | Create, Validate, Revoke, List, SHA256 hash, RevokedAt  |
| `AuditLogServiceTests`  | 6      | Log, filter by action, filter by user, pagination, count, filter by date |

---

## Features

- [x] **ASP.NET Core Identity** -- Registration, login, logout, lockout after 5 attempts (15 min)
- [x] **OAuth2/OIDC (OpenIddict 7.4.0)** -- Authorization Code + PKCE, Client Credentials, Refresh Token
- [x] **API Key Authentication** -- Generation with SHA256 hash, validation via `X-API-Key` header, revocation
- [x] **Two-Factor Authentication (2FA)** -- TOTP with QR Code URI, recovery codes (10 codes)
- [x] **User Management** -- CRUD, enable/disable, lock/unlock, role assignment
- [x] **Role Management** -- Create, list, delete custom roles
- [x] **OAuth Client Management** -- Create public/confidential clients, define redirect URIs and permissions
- [x] **Audit Log** -- Tracking of 16 action types with IP, User-Agent and details
- [x] **Blazor Server Admin Dashboard** -- 7 interactive pages for complete administration
- [x] **React SPA Portal** -- Public OAuth2 client with PKCE for user portal
- [x] **Rate Limiting** -- Fixed window (10 requests/minute) on authentication endpoints
- [x] **Health Checks** -- PostgreSQL connectivity check at `/health`
- [x] **CORS** -- Configured for the React portal with credentials
- [x] **Correlation ID** -- Tracking middleware via `X-Correlation-Id` header
- [x] **Structured Logging** -- Serilog with integrated request logging
- [x] **Automatic Seeding** -- Roles, admin user and OAuth clients created at startup
- [x] **Password Policy** -- Minimum 8 characters, uppercase, lowercase, digit required
- [x] **Unit Tests** -- xUnit + Moq + EF Core InMemory with 13 tests

---

## Audit

The system automatically logs the following actions:

| Action            | Description                        |
|-------------------|------------------------------------|
| `Login`           | Successful login                   |
| `LoginFailed`     | Failed login attempt               |
| `Logout`          | User logout                        |
| `Register`        | New user registration              |
| `EmailConfirmed`  | Email confirmation                 |
| `PasswordReset`   | Password reset                     |
| `PasswordChanged` | Password change                    |
| `Enable2FA`       | 2FA activation                     |
| `Disable2FA`      | 2FA deactivation                   |
| `RoleAssigned`    | Role assignment                    |
| `RoleRemoved`     | Role removal                       |
| `ApiKeyCreated`   | API Key creation                   |
| `ApiKeyRevoked`   | API Key revocation                 |
| `ClientCreated`   | OAuth client creation              |
| `ClientUpdated`   | OAuth client update                |
| `AccountDeleted`  | Account deletion                   |

---

## API Key Authentication

To authenticate via API Key, send the `X-API-Key` header in the request:

```bash
curl -H "X-API-Key: <YOUR_API_KEY>" https://localhost:5001/api/protected-resource
```

Keys are stored as SHA256 hashes. The plaintext key is displayed **only once** at the time of creation.

---

Developed by **RVM Tech**
