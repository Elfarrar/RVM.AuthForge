# RVM.AuthForge

## Visao Geral
Sistema IAM (Identity & Access Management) de portfolio demonstrando autenticacao centralizada com OpenIddict 7.4 e ASP.NET Identity. Suporta 2FA via TOTP, gerenciamento de usuarios/roles/clients via painel Blazor Server, e portal de autenticacao em React (Vite + TypeScript).

Projeto portfolio — sem dados reais. Objetivo: demonstrar arquitetura de SSO, OIDC, PKCE, refresh tokens e boas praticas de seguranca.

## Stack
- .NET 10, ASP.NET Core, Blazor Server (painel admin)
- OpenIddict 7.4 (OIDC provider), ASP.NET Identity
- React 18 + Vite + TypeScript (portal de autenticacao `authforge-portal`)
- PostgreSQL 16 (via Npgsql), Entity Framework Core
- Serilog + Seq, RVM.Common.Security
- xUnit 48 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.AuthForge.API/          # Host: Blazor admin + OIDC endpoints + API controllers
    Auth/                     # ApiKeyAuthHandler
    Components/               # Blazor pages (admin UI)
    Controllers/              # REST endpoints
    Middleware/               # CorrelationId
    Services/                 # SeedService (bootstrap roles/users)
  RVM.AuthForge.Domain/       # Entidades, interfaces
  RVM.AuthForge.Infrastructure/
    Data/                     # AuthForgeDbContext (Identity + OpenIddict)
    Services/                 # Implementacoes (user management, 2FA)
  authforge-portal/           # React portal (Vite, login/register/profile)
test/
  RVM.AuthForge.Tests/        # xUnit (48 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- Auth admin via role `Admin` (politica `options.AddPolicy("Admin", ...)`)
- API externa via ApiKey header (esquema `ApiKey`)
- Rate limiting: 10 req/min em rotas de autenticacao
- CORS `ReactPortal` liberado apenas para `http://localhost:5173`
- Serilog com Seq (opcional via `Seq:ServerUrl`)
- Migrations: `EnsureCreated` em dev; migrations EF Core em producao

## Como Rodar
### Dev
```bash
# Subir dependencias
docker compose -f docker-compose.prod.yml up -d postgres

# API + Blazor admin
cd src/RVM.AuthForge.API
dotnet run

# React portal (em outro terminal)
cd src/authforge-portal
npm install && npm run dev
```

### Testes
```bash
dotnet test
```

## Decisoes Arquiteturais
- **OpenIddict 7.4** em vez de IdentityServer: licenca livre, integracao nativa com ASP.NET Identity, suporte completo a PKCE e device flow
- **Blazor Server para admin** + **React para portal**: admin precisa de interatividade server-side com dados; portal precisa de SPA leve sem autorizacao de rota
- **SeedService como HostedService**: garante roles e users iniciais antes do primeiro request, sem script externo
- **ApiKey separado de OIDC**: permite integracao M2M sem fluxo de usuario
