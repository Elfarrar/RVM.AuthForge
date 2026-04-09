*[English](README.en.md) | **Português***

# RVM.AuthForge

Plataforma de Identity & Access Management construida com ASP.NET Core, OpenIddict e PostgreSQL.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![OpenIddict](https://img.shields.io/badge/OpenIddict-7.4.0-blue)
![License](https://img.shields.io/badge/license-proprietary-red)

---

## Sobre

O **RVM.AuthForge** e um servidor de identidade completo que fornece autenticacao, autorizacao OAuth2/OIDC, gerenciamento de usuarios, API Keys e auditoria para todo o ecossistema RVM Tech. Inclui um painel administrativo Blazor Server integrado e suporta um portal de usuario React SPA como cliente OAuth2.

Principais capacidades:
- Servidor OAuth2/OIDC compativel com Authorization Code + PKCE, Client Credentials e Refresh Tokens
- Autenticacao por API Key com hash SHA256
- 2FA (Two-Factor Authentication) com TOTP e QR Code
- Audit log completo com rastreamento de IP, User-Agent e Correlation ID
- Dashboard administrativo Blazor Server embutido
- Rate limiting por endpoint, health checks e CORS configuravel

---

## Tecnologias

| Camada          | Tecnologia                                         |
|-----------------|-----------------------------------------------------|
| Runtime         | .NET 10.0                                           |
| Web Framework   | ASP.NET Core 10                                     |
| Identity        | ASP.NET Core Identity                               |
| OAuth2/OIDC     | OpenIddict 7.4.0                                    |
| ORM             | Entity Framework Core 10                            |
| Banco de Dados  | PostgreSQL (Npgsql 10.0.1)                           |
| Admin Dashboard | Blazor Server (Interactive Server Components)        |
| Portal Usuario  | React SPA (porta 5173)                               |
| Logging         | Serilog 10.0.0                                       |
| Health Checks   | AspNetCore.HealthChecks.NpgSql                       |
| Testes          | xUnit 2.9.3, Moq 4.20, EF Core InMemory, coverlet  |
| SDK Testes      | Microsoft.NET.Test.Sdk 17.14.0                       |

---

## Arquitetura

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

## Estrutura do Projeto

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

## Como Executar

### Pre-requisitos

- [.NET SDK 10.0.201+](https://dot.net/download)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Node.js 20+](https://nodejs.org/) (para o React Portal, opcional)

### 1. Clonar o repositorio

```bash
git clone https://github.com/rvmtech/RVM.AuthForge.git
cd RVM.AuthForge
```

### 2. Configurar a connection string

Edite `src/RVM.AuthForge.API/appsettings.json` ou defina via variavel de ambiente:

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

### 3. Build e execucao

```bash
dotnet build
dotnet run --project src/RVM.AuthForge.API
```

O servidor ira:
1. Aplicar migrations automaticamente no PostgreSQL
2. Criar as roles `Admin` e `User`
3. Criar o usuario administrador (conforme configuracao)
4. Registrar os clientes OAuth2 pre-configurados

### 4. Acessar

| Recurso             | URL                            |
|---------------------|---------------------------------|
| API                 | `https://localhost:5001`        |
| Admin Dashboard     | `https://localhost:5001/admin`  |
| Health Check        | `https://localhost:5001/health` |
| React Portal (dev)  | `http://localhost:5173`         |

---

## Endpoints da API

### Account (`/api/account`)

| Metodo | Endpoint                    | Descricao                          | Auth             | Rate Limit |
|--------|-----------------------------|------------------------------------|------------------|------------|
| POST   | `/api/account/register`     | Registro de novo usuario           | Publico          | auth (10/min) |
| POST   | `/api/account/login`        | Login com email e senha            | Publico          | auth (10/min) |
| POST   | `/api/account/logout`       | Logout do usuario autenticado      | Bearer Token     | -          |
| GET    | `/api/account/profile`      | Obter perfil do usuario            | Bearer Token     | -          |
| PUT    | `/api/account/profile`      | Atualizar nome e avatar            | Bearer Token     | -          |
| POST   | `/api/account/change-password` | Alterar senha                   | Bearer Token     | -          |
| POST   | `/api/account/forgot-password` | Solicitar reset de senha        | Publico          | auth (10/min) |
| POST   | `/api/account/reset-password`  | Resetar senha com token         | Publico          | auth (10/min) |
| POST   | `/api/account/confirm-email`   | Confirmar email com token       | Publico          | -          |
| GET    | `/api/account/2fa/status`      | Status do 2FA                   | Bearer Token     | -          |
| POST   | `/api/account/2fa/enable`      | Gerar chave TOTP e URI QR Code  | Bearer Token     | -          |
| POST   | `/api/account/2fa/verify`      | Verificar codigo e ativar 2FA   | Bearer Token     | -          |
| POST   | `/api/account/2fa/disable`     | Desativar 2FA                   | Bearer Token     | -          |
| POST   | `/api/account/2fa/recovery-codes` | Gerar novos codigos de recuperacao | Bearer Token | -          |

### Admin (`/api/admin`) -- Requer role `Admin`

| Metodo | Endpoint                          | Descricao                          |
|--------|-----------------------------------|------------------------------------|
| GET    | `/api/admin/users`                | Listar usuarios (paginado, busca)  |
| GET    | `/api/admin/users/{id}`           | Detalhes de um usuario             |
| PUT    | `/api/admin/users/{id}`           | Atualizar nome e status do usuario |
| POST   | `/api/admin/users/{id}/roles`     | Atribuir role a um usuario         |
| DELETE | `/api/admin/users/{id}/roles/{role}` | Remover role de um usuario      |
| POST   | `/api/admin/users/{id}/lock`      | Bloquear usuario                   |
| POST   | `/api/admin/users/{id}/unlock`    | Desbloquear usuario                |
| GET    | `/api/admin/roles`                | Listar todas as roles              |
| POST   | `/api/admin/roles`                | Criar nova role                    |
| DELETE | `/api/admin/roles/{id}`           | Deletar role                       |
| GET    | `/api/admin/api-keys`             | Listar API Keys                    |
| POST   | `/api/admin/api-keys`             | Criar nova API Key                 |
| POST   | `/api/admin/api-keys/{id}/revoke` | Revogar API Key                    |
| GET    | `/api/admin/clients`              | Listar clientes OAuth              |
| POST   | `/api/admin/clients`              | Criar cliente OAuth                |
| PUT    | `/api/admin/clients/{id}`         | Atualizar cliente OAuth            |
| GET    | `/api/admin/audit`                | Consultar audit log (filtros)      |

### OAuth2/OIDC (OpenIddict)

| Metodo   | Endpoint              | Descricao                         |
|----------|-----------------------|-----------------------------------|
| GET/POST | `/connect/authorize`  | Endpoint de autorizacao           |
| POST     | `/connect/token`      | Emissao de tokens                 |
| GET      | `/connect/userinfo`   | Informacoes do usuario autenticado|
| GET/POST | `/connect/logout`     | End session / logout              |

### Infraestrutura

| Metodo | Endpoint    | Descricao                  |
|--------|-------------|----------------------------|
| GET    | `/health`   | Health check (PostgreSQL)  |

---

## Blazor Admin Dashboard

O painel administrativo e servido como Blazor Server interativo, acessivel em `/admin`. Paginas disponiveis:

| Rota                    | Pagina                | Descricao                                       |
|-------------------------|-----------------------|-------------------------------------------------|
| `/admin`                | Dashboard             | Metricas: total de usuarios, ativos, 2FA, eventos 24h. Tabela de atividade recente. |
| `/admin/users`          | Usuarios              | Listagem paginada com busca, lock/unlock de contas. |
| `/admin/users/{id}`     | Detalhe do Usuario    | Info completa, gerenciamento de roles, historico de auditoria. |
| `/admin/roles`          | Roles                 | Criar, listar e deletar roles.                  |
| `/admin/api-keys`       | API Keys              | Criar, listar e revogar API Keys. Chave exibida apenas na criacao. |
| `/admin/clients`        | Clientes OAuth        | Criar e listar clientes OAuth2 (publicos e confidenciais). |
| `/admin/audit`          | Audit Log             | Consulta com filtros por acao, usuario, data. Paginado. |

---

## React Portal

O portal do usuario e um React SPA (client OAuth2 publico) rodando em `http://localhost:5173`. Paginas previstas:

| Pagina       | Descricao                                          |
|--------------|-----------------------------------------------------|
| Login        | Redirect para o Authorization Server via OAuth2 PKCE |
| Callback     | Recebe o authorization code e troca por tokens      |
| Profile      | Visualizar e editar perfil (nome, avatar)           |
| Security     | Alterar senha, ativar/desativar 2FA, recovery codes |
| Logout       | End session via OIDC                                |

**CORS** configurado para `http://localhost:5173` com credentials habilitado.

---

## OAuth2/OIDC

### Fluxos Suportados

| Fluxo                        | Descricao                                       |
|------------------------------|-------------------------------------------------|
| Authorization Code + PKCE    | Para SPAs e aplicacoes publicas. Obrigatorio PKCE. |
| Client Credentials           | Para comunicacao machine-to-machine (M2M).       |
| Refresh Token                | Renovacao de access tokens sem re-autenticacao.  |

### Configuracao de Tokens

| Parametro              | Valor           |
|------------------------|-----------------|
| Access Token Lifetime  | 30 minutos      |
| Refresh Token Lifetime | 7 dias          |
| Encriptacao de AT      | Desabilitada    |
| Scopes registrados     | `openid`, `profile`, `email`, `api` |

### Clientes Pre-Configurados (Seed)

#### authforge-portal (Publico)

| Propriedade       | Valor                                  |
|-------------------|----------------------------------------|
| Client ID         | `authforge-portal`                     |
| Tipo              | Public                                 |
| Display Name      | RVM.AuthForge Portal (React SPA)       |
| Redirect URI      | `http://localhost:5173/callback`        |
| Post-Logout URI   | `http://localhost:5173/`                |
| Grant Types       | Authorization Code, Refresh Token      |
| PKCE              | Obrigatorio                            |
| Scopes            | email, profile, roles, api             |

#### demo-api (Confidencial)

| Propriedade       | Valor                                  |
|-------------------|----------------------------------------|
| Client ID         | `demo-api`                             |
| Client Secret     | `demo-api-secret`                      |
| Tipo              | Confidential                           |
| Display Name      | Demo API (Confidential)                |
| Grant Types       | Client Credentials                     |
| Scopes            | api                                    |

### Exemplo: Authorization Code + PKCE

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

### Exemplo: Client Credentials

```
POST /connect/token
  grant_type=client_credentials&
  client_id=demo-api&
  client_secret=demo-api-secret&
  scope=api
```

---

## Testes

O projeto usa **xUnit** com **Moq** e **EF Core InMemory** para testes unitarios.

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura (coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Executar um teste especifico
dotnet test --filter "FullyQualifiedName~ApiKeyServiceTests"
```

### Testes Existentes

| Classe                  | Testes | Cobertura                                                |
|-------------------------|--------|----------------------------------------------------------|
| `ApiKeyServiceTests`    | 7      | Create, Validate, Revoke, List, SHA256 hash, RevokedAt  |
| `AuditLogServiceTests`  | 6      | Log, filtro por acao, filtro por usuario, paginacao, count, filtro por data |

---

## Funcionalidades

- [x] **ASP.NET Core Identity** -- Registro, login, logout, lockout apos 5 tentativas (15 min)
- [x] **OAuth2/OIDC (OpenIddict 7.4.0)** -- Authorization Code + PKCE, Client Credentials, Refresh Token
- [x] **API Key Authentication** -- Geracao com SHA256 hash, validacao via header `X-API-Key`, revogacao
- [x] **Two-Factor Authentication (2FA)** -- TOTP com QR Code URI, codigos de recuperacao (10 codigos)
- [x] **Gerenciamento de Usuarios** -- CRUD, ativar/desativar, lock/unlock, atribuicao de roles
- [x] **Gerenciamento de Roles** -- Criar, listar, deletar roles customizadas
- [x] **Gerenciamento de Clientes OAuth** -- Criar clientes publicos/confidenciais, definir redirect URIs e permissoes
- [x] **Audit Log** -- Rastreamento de 16 tipos de acoes com IP, User-Agent e detalhes
- [x] **Blazor Server Admin Dashboard** -- 7 paginas interativas para administracao completa
- [x] **React SPA Portal** -- Cliente OAuth2 publico com PKCE para portal do usuario
- [x] **Rate Limiting** -- Fixed window (10 requisicoes/minuto) nos endpoints de autenticacao
- [x] **Health Checks** -- Verificacao de conectividade com PostgreSQL em `/health`
- [x] **CORS** -- Configurado para o portal React com credentials
- [x] **Correlation ID** -- Middleware de rastreamento via header `X-Correlation-Id`
- [x] **Structured Logging** -- Serilog com request logging integrado
- [x] **Seed Automatico** -- Roles, usuario admin e clientes OAuth criados na inicializacao
- [x] **Politica de Senhas** -- Minimo 8 caracteres, maiuscula, minuscula, digito obrigatorios
- [x] **Testes Unitarios** -- xUnit + Moq + EF Core InMemory com 13 testes

---

## Auditoria

O sistema registra automaticamente as seguintes acoes:

| Acao              | Descricao                          |
|-------------------|------------------------------------|
| `Login`           | Login bem-sucedido                 |
| `LoginFailed`     | Tentativa de login falha           |
| `Logout`          | Logout do usuario                  |
| `Register`        | Registro de novo usuario           |
| `EmailConfirmed`  | Confirmacao de email               |
| `PasswordReset`   | Reset de senha                     |
| `PasswordChanged` | Alteracao de senha                 |
| `Enable2FA`       | Ativacao do 2FA                    |
| `Disable2FA`      | Desativacao do 2FA                 |
| `RoleAssigned`    | Atribuicao de role                 |
| `RoleRemoved`     | Remocao de role                    |
| `ApiKeyCreated`   | Criacao de API Key                 |
| `ApiKeyRevoked`   | Revogacao de API Key               |
| `ClientCreated`   | Criacao de cliente OAuth           |
| `ClientUpdated`   | Atualizacao de cliente OAuth       |
| `AccountDeleted`  | Exclusao de conta                  |

---

## API Key Authentication

Para autenticar via API Key, envie o header `X-API-Key` na requisicao:

```bash
curl -H "X-API-Key: <SUA_API_KEY>" https://localhost:5001/api/recurso-protegido
```

As chaves sao armazenadas como hash SHA256. A chave em texto plano e exibida **apenas uma vez** no momento da criacao.

---

Desenvolvido por **RVM Tech**
