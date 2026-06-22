# Segurança da API

## Visão geral

A camada de segurança do FiscalFlow combina:

- autenticação JWT Bearer;
- validação de issuer, audience, assinatura e expiração;
- autorização por política;
- exigência das claims `sub` e `tenant_id`;
- rate limiting por usuário autenticado ou endereço IP;
- respostas padronizadas com `ProblemDetails` para 401, 403 e 429.

A segurança permanece desabilitada por padrão para preservar o ambiente local atual. Em ambientes protegidos, habilite-a por configuração e forneça a chave exclusivamente por variável de ambiente ou secret store.

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

## Claims obrigatórias

Todo token usado nos endpoints fiscais deve possuir:

- `sub`: identificador estável do usuário ou integração;
- `tenant_id`: tenant que será aplicado ao contexto da requisição.

Quando a segurança estiver habilitada, os endpoints fiscais exigem a política `fiscalflow-api`. Os endpoints de saúde permanecem públicos.

## Respostas

- `401 Unauthorized`: token ausente, inválido, expirado ou com assinatura incorreta;
- `403 Forbidden`: token válido sem as claims obrigatórias;
- `429 Too Many Requests`: limite de requisições excedido.

As respostas incluem o correlation ID da requisição.

## Rate limiting

O limite usa janela fixa. A partição é resolvida por:

1. claim `sub`, quando autenticada;
2. endereço IP remoto;
3. chave `anonymous`, quando nenhum dos anteriores estiver disponível.

A resposta 429 inclui `Retry-After` quando o limitador fornece essa informação.

## Recomendações de produção

- use uma chave aleatória longa e exclusiva por ambiente;
- armazene segredos em secret store da plataforma;
- rotacione a chave periodicamente;
- utilize HTTPS em todo o tráfego externo;
- mantenha issuer e audience específicos para a API;
- revise os limites com testes de carga antes do deploy;
- não registre tokens ou chaves nos logs.
