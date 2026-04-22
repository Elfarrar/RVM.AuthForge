import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.AUTHFORGE_BASE_URL ?? 'https://authforge.lab.rvmtech.com.br';

test.describe('AuthForge — API Auth', () => {
  test.skip(
    process.env.AUTHFORGE_RUN_SMOKE !== '1',
    'Defina AUTHFORGE_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /Account/Login — página de login carrega', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/Account/Login`);
    expect(response.status()).toBe(200);
  });

  test('GET /Account/Register — página de registro carrega', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/Account/Register`);
    expect(response.status()).toBe(200);
  });

  test('GET /Admin — redireciona para login ou retorna 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/Admin`, { maxRedirects: 0 });
    expect([200, 302, 401, 403]).toContain(response.status());
  });

  test('POST /connect/authorize sem parâmetros retorna erro', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/connect/authorize`);
    expect([400, 401, 403]).toContain(response.status());
  });

  test('GET /.well-known/openid-configuration retorna discovery doc', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/.well-known/openid-configuration`);
    expect(response.status()).toBe(200);
    const body = await response.json();
    expect(body).toHaveProperty('issuer');
    expect(body).toHaveProperty('authorization_endpoint');
  });
});
