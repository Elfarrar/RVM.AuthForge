/**
 * RVM.AuthForge — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema IAM,
 * captura screenshots em desktop e mobile, e gera as imagens para o manual.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';

const BASE_URL = process.env.AUTHFORGE_BASE_URL ?? 'https://authforge.lab.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`), fullPage });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`), fullPage });
  await page.setViewportSize({ width: 1280, height: 800 });
}

// ---------------------------------------------------------------------------
// 1. Blazor Admin — telas publicas
// ---------------------------------------------------------------------------
test.describe('1. Blazor Admin — Publico', () => {
  test('1.1 Home', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-home');
  });
});

// ---------------------------------------------------------------------------
// 2. Blazor Admin — painel admin
// ---------------------------------------------------------------------------
test.describe('2. Blazor Admin — Painel', () => {
  test('2.1 Dashboard Admin', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-admin-dashboard');
  });

  test('2.2 Usuarios', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-admin-users');
  });

  test('2.3 Roles', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin/roles`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-admin-roles');
  });

  test('2.4 Clients OIDC', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin/clients`);
    await page.waitForLoadState('networkidle');
    await capture(page, '05-admin-clients');
  });

  test('2.5 API Keys', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin/api-keys`);
    await page.waitForLoadState('networkidle');
    await capture(page, '06-admin-api-keys');
  });

  test('2.6 Audit Log', async ({ page }) => {
    await page.goto(`${BASE_URL}/admin/audit`);
    await page.waitForLoadState('networkidle');
    await capture(page, '07-admin-audit');
  });
});

// ---------------------------------------------------------------------------
// 3. React Portal — telas de autenticacao
// ---------------------------------------------------------------------------
test.describe('3. React Portal', () => {
  test('3.1 Portal Login', async ({ page }) => {
    await page.goto(`${BASE_URL}/portal`);
    await page.waitForLoadState('networkidle');
    await capture(page, '08-portal-login');
  });

  test('3.2 Portal Registro', async ({ page }) => {
    await page.goto(`${BASE_URL}/portal/register`);
    await page.waitForLoadState('networkidle');
    await capture(page, '09-portal-register');
  });

  test('3.3 Portal Esqueci Senha', async ({ page }) => {
    await page.goto(`${BASE_URL}/portal/forgot-password`);
    await page.waitForLoadState('networkidle');
    await capture(page, '10-portal-forgot-password');
  });

  test('3.4 Portal Perfil', async ({ page }) => {
    await page.goto(`${BASE_URL}/portal/profile`);
    await page.waitForLoadState('networkidle');
    await capture(page, '11-portal-profile');
  });

  test('3.5 Portal 2FA', async ({ page }) => {
    await page.goto(`${BASE_URL}/portal/two-factor`);
    await page.waitForLoadState('networkidle');
    await capture(page, '12-portal-2fa');
  });
});
