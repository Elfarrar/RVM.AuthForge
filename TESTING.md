# Testes — RVM.AuthForge

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.AuthForge.Tests/`
  - `Controllers/` — testes de controllers REST
  - `Services/` — testes de servicos de autenticacao e 2FA
- **Total:** 48 testes

```bash
dotnet test test/RVM.AuthForge.Tests/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** painel Blazor admin + fluxos de login/2FA no React portal

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
AUTHFORGE_BASE_URL=http://localhost:5000
AUTHFORGE_ADMIN_EMAIL=admin@authforge.local
AUTHFORGE_ADMIN_PASSWORD=<senha>
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- Playwright com cache de browsers (`playwright-ci-caching`)
