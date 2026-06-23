# Índice da documentação

Este arquivo registra a estrutura e a revisão da documentação do FiscalFlow.

## Documentos principais

| Arquivo | Conteúdo |
|---|---|
| [`../README.md`](../README.md) | Visão geral, quick start e links |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | Camadas, fluxos e modelo de domínio |
| [`API.md`](API.md) | Referência completa da API |
| [`ENDPOINTS.md`](ENDPOINTS.md) | Catálogo rápido de endpoints |
| [`CONFIGURATION.md`](CONFIGURATION.md) | Variáveis de ambiente e ambientes |
| [`SECURITY.md`](SECURITY.md) | JWT, autorização e rate limiting |
| [`OBSERVABILITY.md`](OBSERVABILITY.md) | Logs, métricas, tracing e health |
| [`ERROR_RESPONSES.md`](ERROR_RESPONSES.md) | Formato e códigos de erro |
| [`HANGFIRE_TIMEOUT.md`](HANGFIRE_TIMEOUT.md) | Jobs recorrentes e timeout |
| [`REQUEST_ID.md`](REQUEST_ID.md) | Correlation ID |
| [`DEVELOPMENT.md`](DEVELOPMENT.md) | Fluxo de branches e convenções |
| [`ROADMAP.md`](ROADMAP.md) | Progresso e próximas etapas |
| [`PROJECT-DESCRIPTIONS.md`](PROJECT-DESCRIPTIONS.md) | Textos para portfólio |

## Outros recursos

| Recurso | Local |
|---|---|
| Coleção de requisições | [`requests/`](../requests/) |
| Workflow CI | [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) |
| Docker Compose | [`docker-compose.yml`](../docker-compose.yml) |

## Última revisão

Documentação atualizada para refletir o estado do projeto em junho de 2026:

- processamento assíncrono com RabbitMQ implementado;
- jobs Hangfire para retry e detecção de timeout;
- upload e parsing seguro de XML fiscal;
- observabilidade com OpenTelemetry;
- autenticação JWT e rate limiting (opcionais, desabilitados por padrão);
- Docker Compose com MongoDB, RabbitMQ e API;
- catálogo de endpoints em `ENDPOINTS.md`.

Pendências documentadas: dashboard Hangfire, testes E2E ampliados e deploy.
