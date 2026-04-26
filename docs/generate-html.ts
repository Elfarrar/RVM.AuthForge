/**
 * RVM.AuthForge — Gerador de Manual HTML
 *
 * Le os screenshots gerados pelo Playwright e produz um manual HTML standalone
 * com descritivos de cada funcionalidade.
 *
 * Uso:
 *   cd docs && npx tsx generate-html.ts
 *
 * Saida:
 *   docs/manual-usuario.html
 *   docs/manual-usuario.md
 */
import fs from 'fs';
import path from 'path';

const SCREENSHOTS_DIR = path.resolve(__dirname, 'screenshots');
const OUTPUT_HTML = path.resolve(__dirname, 'manual-usuario.html');
const OUTPUT_MD = path.resolve(__dirname, 'manual-usuario.md');

interface Section {
  id: string;
  title: string;
  description: string;
  screenshot: string;
  features: string[];
  tips?: string[];
}

const sections: Section[] = [
  // --- Blazor Admin ---
  {
    id: 'home',
    title: '1. Pagina Inicial',
    description:
      'Pagina de entrada do RVM.AuthForge. Apresenta o sistema IAM e oferece ' +
      'acesso ao painel administrativo e ao portal de autenticacao React.',
    screenshot: '01-home',
    features: [
      'Apresentacao do sistema IAM',
      'Link para painel admin Blazor',
      'Link para portal de autenticacao React',
      'Status do servico de autenticacao',
    ],
  },
  {
    id: 'admin-dashboard',
    title: '2. Dashboard Administrativo',
    description:
      'Painel central do administrador. Exibe estatisticas globais do sistema: ' +
      'total de usuarios, roles configuradas, clients OIDC registrados e atividade recente.',
    screenshot: '02-admin-dashboard',
    features: [
      'Contagem total de usuarios, roles e clients',
      'Grafico de atividade de login recente',
      'Alertas de seguranca (tentativas de login falhas)',
      'Navegacao lateral para todas as secoes admin',
    ],
  },
  {
    id: 'admin-users',
    title: '3. Gerenciamento de Usuarios',
    description:
      'Lista completa de usuarios cadastrados no sistema. Permite busca, filtro por status, ' +
      'visualizacao de detalhes, edicao de roles e desativacao de contas.',
    screenshot: '03-admin-users',
    features: [
      'Listagem paginada de usuarios',
      'Busca por nome ou e-mail',
      'Filtro por status (ativo/inativo)',
      'Atribuicao e remocao de roles',
      'Desativacao de conta sem exclusao',
      'Visualizacao de detalhes individuais',
    ],
    tips: [
      'Use o filtro de roles para encontrar rapidamente administradores ou usuarios de modulos especificos.',
    ],
  },
  {
    id: 'admin-roles',
    title: '4. Gerenciamento de Roles',
    description:
      'Configuracao das roles de acesso do sistema. Roles controlam quais funcionalidades ' +
      'cada usuario pode acessar nos diferentes modulos.',
    screenshot: '04-admin-roles',
    features: [
      'Criacao de novas roles',
      'Visualizacao de usuarios por role',
      'Edicao de descricao e permissoes',
      'Remocao de roles nao utilizadas',
    ],
  },
  {
    id: 'admin-clients',
    title: '5. Clients OIDC',
    description:
      'Gerenciamento de aplicacoes clientes registradas no provedor OIDC. ' +
      'Cada client representa uma aplicacao que usa o AuthForge para autenticacao.',
    screenshot: '05-admin-clients',
    features: [
      'Listagem de clients registrados (Confidential/Public)',
      'Configuracao de redirect URIs',
      'Definicao de scopes permitidos',
      'Rotacao de client secrets',
      'Suporte a Authorization Code Flow + PKCE',
    ],
    tips: [
      'Clients publicos (SPAs/mobile) devem usar PKCE sem client secret.',
      'Clients confidenciais (server-side) usam client secret.',
    ],
  },
  {
    id: 'admin-api-keys',
    title: '6. API Keys',
    description:
      'Gerenciamento de chaves de API para integracao M2M (Machine-to-Machine). ' +
      'As API Keys permitem que servicos backend acessem endpoints protegidos sem fluxo de usuario.',
    screenshot: '06-admin-api-keys',
    features: [
      'Geracao de novas API Keys (plain text exibido apenas uma vez)',
      'Rotulo e descricao por chave',
      'Revogacao imediata de chaves comprometidas',
      'Armazenamento com hash SHA-256 (seguranca)',
    ],
    tips: [
      'Copie a chave no momento da criacao — ela nao sera exibida novamente.',
      'Revogue imediatamente qualquer chave que tenha sido exposta.',
    ],
  },
  {
    id: 'admin-audit',
    title: '7. Log de Auditoria',
    description:
      'Registro cronologico de todas as acoes relevantes no sistema: logins, ' +
      'alteracoes de usuarios, criacao de clients e uso de API Keys.',
    screenshot: '07-admin-audit',
    features: [
      'Historico completo de eventos de seguranca',
      'Filtro por tipo de evento, usuario e periodo',
      'Registro de IP e user-agent por evento',
      'Exportacao de logs para analise',
    ],
  },
  // --- React Portal ---
  {
    id: 'portal-login',
    title: '8. Portal — Login',
    description:
      'Tela de login do portal React. Interface moderna e responsiva para ' +
      'autenticacao dos usuarios finais via OIDC Authorization Code Flow.',
    screenshot: '08-portal-login',
    features: [
      'Login com e-mail e senha',
      'Suporte a fluxo OIDC completo (Authorization Code + PKCE)',
      'Redirecionamento apos autenticacao',
      'Link para registro e recuperacao de senha',
    ],
  },
  {
    id: 'portal-register',
    title: '9. Portal — Registro',
    description:
      'Criacao de nova conta pelo portal React. ' +
      'Validacao em tempo real e feedback imediato ao usuario.',
    screenshot: '09-portal-register',
    features: [
      'Campos: nome, e-mail, senha e confirmacao',
      'Validacao de forca de senha em tempo real',
      'E-mail de confirmacao enviado apos cadastro',
      'Redirecionamento automatico apos registro',
    ],
  },
  {
    id: 'portal-forgot-password',
    title: '10. Portal — Recuperacao de Senha',
    description:
      'Fluxo de recuperacao de senha pelo portal. ' +
      'O usuario recebe um link por e-mail para redefinir a senha com seguranca.',
    screenshot: '10-portal-forgot-password',
    features: [
      'Envio de link de redefinicao por e-mail',
      'Link com validade de 24 horas',
      'Confirmacao visual do envio do e-mail',
    ],
  },
  {
    id: 'portal-profile',
    title: '11. Portal — Perfil',
    description:
      'Gerenciamento de dados pessoais pelo portal. ' +
      'O usuario pode atualizar nome, e-mail e outras informacoes da conta.',
    screenshot: '11-portal-profile',
    features: [
      'Edicao de nome e e-mail',
      'Status de verificacao de e-mail',
      'Informacoes de sessao ativa',
      'Link para alteracao de senha',
    ],
  },
  {
    id: 'portal-2fa',
    title: '12. Portal — Autenticacao em Dois Fatores',
    description:
      'Configuracao e gerenciamento do 2FA pelo portal React. ' +
      'Suporte a aplicativos autenticadores TOTP (Google Authenticator, Authy, etc.).',
    screenshot: '12-portal-2fa',
    features: [
      'Ativacao/desativacao do 2FA',
      'QR Code para configurar aplicativo autenticador',
      'Codigos de recuperacao de emergencia',
      'Status do 2FA visivel no painel',
    ],
    tips: [
      'Ative o 2FA para maior seguranca da sua conta.',
      'Guarde os codigos de recuperacao em local seguro.',
    ],
  },
];

// ---------------------------------------------------------------------------
// Utilitarios
// ---------------------------------------------------------------------------
function imageToBase64(filePath: string): string | null {
  if (!fs.existsSync(filePath)) return null;
  const buffer = fs.readFileSync(filePath);
  return `data:image/png;base64,${buffer.toString('base64')}`;
}

function generateHTML(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let sectionsHtml = '';
  for (const s of sections) {
    const desktopPath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`);
    const mobilePath = path.join(SCREENSHOTS_DIR, `${s.screenshot}--mobile.png`);
    const desktopImg = imageToBase64(desktopPath);
    const mobileImg = imageToBase64(mobilePath);

    const featuresHtml = s.features.map((f) => `<li>${f}</li>`).join('\n            ');
    const tipsHtml = s.tips
      ? `<div class="tips">
          <strong>Dicas:</strong>
          <ul>${s.tips.map((t) => `<li>${t}</li>`).join('\n            ')}</ul>
        </div>`
      : '';

    const screenshotsHtml = desktopImg
      ? `<div class="screenshots">
          <div class="screenshot-group">
            <span class="badge">Desktop</span>
            <img src="${desktopImg}" alt="${s.title} - Desktop" />
          </div>
          ${
            mobileImg
              ? `<div class="screenshot-group mobile">
              <span class="badge">Mobile</span>
              <img src="${mobileImg}" alt="${s.title} - Mobile" />
            </div>`
              : ''
          }
        </div>`
      : '<p class="no-screenshot"><em>Screenshot nao disponivel. Execute o script Playwright para gerar.</em></p>';

    sectionsHtml += `
    <section id="${s.id}">
      <h2>${s.title}</h2>
      <p class="description">${s.description}</p>
      <div class="features">
        <strong>Funcionalidades:</strong>
        <ul>
            ${featuresHtml}
        </ul>
      </div>
      ${tipsHtml}
      ${screenshotsHtml}
    </section>`;
  }

  return `<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>RVM.AuthForge - Manual do Usuario</title>
  <style>
    :root {
      --primary: #2f6fed;
      --surface: #ffffff;
      --bg: #f4f6fa;
      --text: #1e293b;
      --text-muted: #64748b;
      --border: #e2e8f0;
      --sidebar-bg: #0f172a;
      --accent: #3b82f6;
    }
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      background: var(--bg);
      color: var(--text);
      line-height: 1.6;
    }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem 1.5rem; }
    header {
      background: var(--sidebar-bg);
      color: white;
      padding: 3rem 1.5rem;
      text-align: center;
    }
    header h1 { font-size: 2rem; margin-bottom: 0.5rem; }
    header p { color: #94a3b8; font-size: 1rem; }
    header .version { color: #64748b; font-size: 0.85rem; margin-top: 0.5rem; }
    nav {
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      padding: 1rem 1.5rem;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    nav .container { padding: 0; }
    nav ul { list-style: none; display: flex; flex-wrap: wrap; gap: 0.5rem; }
    nav a {
      display: inline-block;
      padding: 0.35rem 0.75rem;
      border-radius: 0.5rem;
      font-size: 0.85rem;
      color: var(--text);
      text-decoration: none;
      background: var(--bg);
      transition: background 0.2s;
    }
    nav a:hover { background: var(--primary); color: white; }
    section {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 1rem;
      padding: 2rem;
      margin-bottom: 2rem;
    }
    section h2 {
      font-size: 1.5rem;
      color: var(--primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid var(--border);
    }
    .description { font-size: 1.05rem; margin-bottom: 1.25rem; color: var(--text); }
    .features, .tips {
      background: var(--bg);
      border-radius: 0.75rem;
      padding: 1rem 1.25rem;
      margin-bottom: 1.25rem;
    }
    .features ul, .tips ul { margin-top: 0.5rem; padding-left: 1.25rem; }
    .features li, .tips li { margin-bottom: 0.35rem; }
    .tips { background: #eff6ff; border-left: 4px solid var(--accent); }
    .tips strong { color: var(--accent); }
    .screenshots {
      display: flex;
      gap: 1.5rem;
      margin-top: 1rem;
      align-items: flex-start;
    }
    .screenshot-group {
      position: relative;
      flex: 1;
      border: 1px solid var(--border);
      border-radius: 0.75rem;
      overflow: hidden;
    }
    .screenshot-group.mobile { flex: 0 0 200px; max-width: 200px; }
    .screenshot-group img { width: 100%; display: block; }
    .badge {
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      background: var(--sidebar-bg);
      color: white;
      font-size: 0.7rem;
      padding: 0.2rem 0.5rem;
      border-radius: 0.35rem;
      font-weight: 600;
      text-transform: uppercase;
    }
    .no-screenshot {
      background: var(--bg);
      padding: 2rem;
      border-radius: 0.75rem;
      text-align: center;
      color: var(--text-muted);
    }
    footer {
      text-align: center;
      padding: 2rem 1rem;
      color: var(--text-muted);
      font-size: 0.85rem;
    }
    @media (max-width: 768px) {
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 100%; flex: 1; }
      section { padding: 1.25rem; }
    }
    @media print {
      nav { display: none; }
      section { break-inside: avoid; page-break-inside: avoid; }
      .screenshots { flex-direction: column; }
      .screenshot-group.mobile { max-width: 250px; }
    }
  </style>
</head>
<body>
  <header>
    <h1>RVM.AuthForge - Manual do Usuario</h1>
    <p>Sistema IAM com OpenIddict — Guia Completo de Funcionalidades</p>
    <div class="version">Gerado em ${now} | RVM Tech</div>
  </header>

  <nav>
    <div class="container">
      <ul>
        ${sections.map((s) => `<li><a href="#${s.id}">${s.title}</a></li>`).join('\n        ')}
      </ul>
    </div>
  </nav>

  <div class="container">
    <section id="visao-geral">
      <h2>Visao Geral</h2>
      <p class="description">
        O <strong>RVM.AuthForge</strong> e um sistema IAM (Identity &amp; Access Management)
        de portfolio que demonstra autenticacao centralizada com OpenIddict 7.4 e ASP.NET Identity.
        Inclui um painel administrativo em Blazor Server e um portal de autenticacao em React.
      </p>
      <div class="features">
        <strong>Recursos principais:</strong>
        <ul>
          <li><strong>OIDC completo</strong> — Authorization Code Flow + PKCE, refresh tokens</li>
          <li><strong>Painel admin Blazor</strong> — gerenciamento de usuarios, roles e clients</li>
          <li><strong>Portal React</strong> — login, registro, perfil e 2FA para usuarios finais</li>
          <li><strong>2FA via TOTP</strong> — suporte a Google Authenticator, Authy e similares</li>
          <li><strong>API Keys</strong> — integracao M2M com autenticacao por chave</li>
          <li><strong>Audit Log</strong> — rastreabilidade completa de eventos de seguranca</li>
        </ul>
      </div>
    </section>

    ${sectionsHtml}
  </div>

  <footer>
    <p>RVM Tech &mdash; Sistema IAM Portfolio</p>
    <p>Documento gerado automaticamente com Playwright + TypeScript</p>
  </footer>
</body>
</html>`;
}

function generateMarkdown(): string {
  const now = new Date().toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });

  let md = `# RVM.AuthForge - Manual do Usuario

> Sistema IAM com OpenIddict — Guia Completo de Funcionalidades
>
> Gerado em ${now} | RVM Tech

---

## Visao Geral

O **RVM.AuthForge** e um sistema IAM de portfolio com OpenIddict 7.4, Blazor Server (admin) e React (portal).

**Recursos principais:**
- **OIDC completo** — Authorization Code Flow + PKCE, refresh tokens
- **Painel admin Blazor** — gerenciamento de usuarios, roles e clients
- **Portal React** — login, registro, perfil e 2FA para usuarios finais
- **2FA via TOTP** — suporte a Google Authenticator, Authy e similares
- **API Keys** — integracao M2M com autenticacao por chave
- **Audit Log** — rastreabilidade completa de eventos de seguranca

---

`;

  for (const s of sections) {
    const desktopExists = fs.existsSync(path.join(SCREENSHOTS_DIR, `${s.screenshot}--desktop.png`));

    md += `## ${s.title}\n\n`;
    md += `${s.description}\n\n`;
    md += `**Funcionalidades:**\n`;
    for (const f of s.features) md += `- ${f}\n`;
    md += '\n';

    if (s.tips) {
      md += `> **Dicas:**\n`;
      for (const t of s.tips) md += `> - ${t}\n`;
      md += '\n';
    }

    if (desktopExists) {
      md += `| Desktop | Mobile |\n|---------|--------|\n`;
      md += `| ![${s.title} - Desktop](screenshots/${s.screenshot}--desktop.png) | ![${s.title} - Mobile](screenshots/${s.screenshot}--mobile.png) |\n`;
    } else {
      md += `*Screenshot nao disponivel. Execute o script Playwright para gerar.*\n`;
    }
    md += '\n---\n\n';
  }

  md += `## Informacoes Tecnicas

| Item | Detalhe |
|------|---------|
| **Tecnologia** | ASP.NET Core + Blazor Server (admin) + React 18 (portal) |
| **Autenticacao** | OpenIddict 7.4 (OAuth 2.0 / OpenID Connect) |
| **Banco de dados** | PostgreSQL 16 + ASP.NET Identity |
| **2FA** | TOTP (RFC 6238) via aplicativos autenticadores |
| **M2M** | API Keys com hash SHA-256 |

---

*Documento gerado automaticamente com Playwright + TypeScript — RVM Tech*
`;

  return md;
}

const html = generateHTML();
fs.writeFileSync(OUTPUT_HTML, html, 'utf-8');
console.log(`HTML gerado: ${OUTPUT_HTML}`);

const md = generateMarkdown();
fs.writeFileSync(OUTPUT_MD, md, 'utf-8');
console.log(`Markdown gerado: ${OUTPUT_MD}`);
