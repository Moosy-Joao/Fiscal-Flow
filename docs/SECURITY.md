# Segurança da API

## Visão geral

A camada de segurança do FiscalFlow combina:

- autenticação JWT Bearer;
- validação de issuer, audience, assinatura e expiração;
- autorização por política `fiscalflow-api`;
- exigência das claims `sub` e `tenant_id`;
- rate limiting por usuário autenticado ou endereço IP;
- respostas padronizadas com `ProblemDetails` para 401, 403 e 429.

A segurança permanece **desabilitada por padrão** (`Security:Enabled=false`) para preservar o fluxo local com `X-Tenant-Id`. Em ambientes protegidos, habilite-a por configuração e forneça a chave exclusivamente por variável de ambiente ou secret store.

## Configuração

```text
Security__Enabled=true
Security__Issuer=FiscalFlow
Security__Audience=FiscalFlow.Api
Security__SigningKey=<segredo com ao menos 32 bytes>
Security__ClockSkewSeconds=60
Security__RateLimitPermitLimit=60
Security__RateLimitWindowSeconds=60
```

`Security__SigningKey` nunca deve ser versionada em arquivos de configuração.

Quando `Security:Enabled=false`, a API ainda registra autenticação JWT e rate limiting internamente, mas os endpoints fiscais não exigem token — apenas o tenant via cabeçalho.

## Claims obrigatórias

Todo token usado nos endpoints fiscais (com segurança habilitada) deve possuir:

| Claim | Descrição |
|---|---|
| `sub` | Identificador estável do usuário ou integração |
| `tenant_id` | Tenant aplicado ao contexto da requisição |

Quando a segurança estiver habilitada, os endpoints fiscais exigem a política `fiscalflow-api`. Os endpoints de saúde (`/api/health`, `/health/live`, `/health/ready`) permanecem públicos e isentos de rate limiting.

## Resolução de tenant

| Modo | Fonte do tenant |
|---|---|
| Autenticado | Claim `tenant_id` (ignora `X-Tenant-Id`) |
| Anônimo (segurança desabilitada) | Cabeçalho `X-Tenant-Id` |

## Respostas

| Status | Situação |
|---|---|
| `401 Unauthorized` | Token ausente, inválido, expirado ou com assinatura incorreta |
| `403 Forbidden` | Token válido sem as claims obrigatórias |
| `429 Too Many Requests` | Limite de requisições excedido |

As respostas incluem o correlation ID da requisição.

## Rate limiting

O limite usa janela fixa (`FixedWindowRateLimiter`). A partição é resolvida por:

1. claim `sub`, quando autenticada;
2. endereço IP remoto;
3. chave `anonymous`, quando nenhum dos anteriores estiver disponível.

A resposta `429` inclui `Retry-After` quando o limitador fornece essa informação.

O rate limiting é aplicado a todos os controllers, exceto endpoints de saúde (decorados com `[DisableRateLimiting]`).

## Exemplo de token (desenvolvimento)

Para testes locais com segurança habilitada, gere um JWT HS256 com:

- `iss`: valor de `Security:Issuer`;
- `aud`: valor de `Security:Audience`;
- `sub`: identificador do usuário;
- `tenant_id`: tenant desejado;
- expiração futura;
- assinatura com `Security:SigningKey`.

Requisição:

```http
GET /api/fiscal-documents HTTP/1.1
Authorization: Bearer <token>
```

## Dashboard Hangfire

O filtro `HangfireDashboardAuthorizationFilter` exige autenticação para acesso ao dashboard, mas a exposição do dashboard ainda não está configurada em `Program.cs`. Esta proteção será ativada quando o dashboard for exposto.

## Recomendações de produção

- use uma chave aleatória longa e exclusiva por ambiente;
- armazene segredos em secret store da plataforma;
- rotacione a chave periodicamente;
- utilize HTTPS em todo o tráfego externo;
- mantenha issuer e audience específicos para a API;
- revise os limites com testes de carga antes do deploy;
- não registre tokens ou chaves nos logs.

## Testes

Testes de segurança rodam no ambiente `SecurityTesting` com rate limit reduzido (`RateLimitPermitLimit=2`). Consulte `tests/FiscalFlow.IntegrationTests/SecurityEndpointTests.cs`.
