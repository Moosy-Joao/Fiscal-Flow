# Configuração por ambiente

O ASP.NET Core converte `:` em `__` nos nomes das variáveis de ambiente. Por exemplo, `MongoDb:DatabaseName` pode ser definido como `MongoDb__DatabaseName`.

## Ambientes

| Ambiente | Uso |
|---|---|
| `Development` | Desenvolvimento local com MongoDB e RabbitMQ |
| `Testing` | Testes de integração (RabbitMQ e Hangfire desabilitados) |
| `SecurityTesting` | Testes de autenticação, autorização e rate limiting |
| `Docker` | Execução via Docker Compose (requer variáveis de conexão) |

Arquivos de configuração em `src/FiscalFlow.Api/`:

- `appsettings.json` — valores base;
- `appsettings.Development.json` — dependências locais;
- `appsettings.Testing.json` — testes de integração;
- `appsettings.SecurityTesting.json` — testes de segurança.

## Aplicação

| Chave | Descrição |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação |
| `ASPNETCORE_URLS` | Endereços HTTP escutados (padrão local: `http://localhost:5298`; Docker: `http://+:8080`) |
| `FiscalDocumentUpload__MaxFileSizeBytes` | Limite do arquivo XML (padrão: 2097152 = 2 MB) |

## MongoDB

| Chave | Descrição |
|---|---|
| `MongoDb__ConnectionString` | Endereço do serviço |
| `MongoDb__DatabaseName` | Banco principal de documentos |
| `MongoDb__InitializeIndexes` | Criação dos índices na inicialização |

Exemplo local:

```text
MongoDb__ConnectionString=mongodb://localhost:27017
MongoDb__DatabaseName=fiscalflow
MongoDb__InitializeIndexes=true
```

## RabbitMQ

A seção `RabbitMq` controla conexão, topologia e retry.

| Chave | Descrição |
|---|---|
| `RabbitMq__Enabled` | Habilita publicação e consumidor (padrão: `true`) |
| `RabbitMq__HostName` | Host do broker |
| `RabbitMq__Port` | Porta AMQP |
| `RabbitMq__UserName` / `Password` | Credenciais |
| `RabbitMq__VirtualHost` | Virtual host |
| `RabbitMq__ExchangeName` | Exchange principal |
| `RabbitMq__QueueName` | Fila de processamento |
| `RabbitMq__RoutingKey` | Routing key principal |
| `RabbitMq__RetryExchangeName` | Exchange de retry |
| `RabbitMq__RetryQueueName` | Fila de retry |
| `RabbitMq__RetryRoutingKey` | Routing key de retry |
| `RabbitMq__DeadLetterExchangeName` | Exchange de dead-letter |
| `RabbitMq__DeadLetterQueueName` | Fila de dead-letter |
| `RabbitMq__DeadLetterRoutingKey` | Routing key de dead-letter |
| `RabbitMq__RetryDelayMilliseconds` | Intervalo entre tentativas |
| `RabbitMq__MaxRetryAttempts` | Máximo de retries antes da DLQ |

Credenciais padrão do Docker Compose: usuário e senha `fiscalflow`.

## Hangfire

A seção `BackgroundJobs` controla jobs recorrentes.

| Chave | Descrição |
|---|---|
| `BackgroundJobs__Enabled` | Habilita Hangfire e jobs recorrentes |
| `BackgroundJobs__DatabaseName` | Banco MongoDB para storage Hangfire |
| `BackgroundJobs__CollectionPrefix` | Prefixo das coleções |
| `BackgroundJobs__WorkerCount` | Quantidade de workers |
| `BackgroundJobs__FailedRetryCron` | Cron do job de reprocessamento |
| `BackgroundJobs__FailedBatchSize` | Lote por execução de retry |
| `BackgroundJobs__MaximumFailedAttempts` | Tentativas máximas por job Hangfire |
| `BackgroundJobs__TimedOutProcessingCron` | Cron da detecção de timeout |
| `BackgroundJobs__TimedOutProcessingBatchSize` | Lote por execução de timeout |
| `BackgroundJobs__ProcessingTimeoutMinutes` | Minutos em `Processing` antes de marcar como `Failed` |

## Observabilidade

| Chave | Descrição |
|---|---|
| `Observability__ServiceName` | Nome do serviço exportado |
| `Observability__ServiceVersion` | Versão informada ao coletor |
| `Observability__OtlpEnabled` | Habilita exportação OTLP |
| `Observability__OtlpEndpoint` | Endereço absoluto do coletor OTLP |

A exportação permanece desabilitada quando `OtlpEnabled` é falso. Health checks e logs estruturados continuam ativos independentemente do coletor. Detalhes em [`OBSERVABILITY.md`](OBSERVABILITY.md).

## Segurança

| Chave | Descrição |
|---|---|
| `Security__Enabled` | Habilita autenticação e autorização nos endpoints fiscais |
| `Security__Issuer` | Emissor esperado no JWT |
| `Security__Audience` | Audience esperada no JWT |
| `Security__SigningKey` | Segredo HMAC; mínimo de 32 bytes |
| `Security__ClockSkewSeconds` | Tolerância de relógio (0–300 segundos) |
| `Security__RateLimitPermitLimit` | Requisições máximas por janela |
| `Security__RateLimitWindowSeconds` | Duração da janela de rate limiting |

A chave de assinatura não deve ser adicionada a `appsettings.json`. Em desenvolvimento, use user-secrets ou variáveis locais. Em hospedagem, utilize o secret store da plataforma.

Exemplo:

```text
Security__Enabled=true
Security__Issuer=FiscalFlow
Security__Audience=FiscalFlow.Api
Security__SigningKey=<segredo com ao menos 32 bytes>
```

Detalhes adicionais estão em [`SECURITY.md`](SECURITY.md).

## Docker Compose

O arquivo `docker-compose.yml` sobe MongoDB, RabbitMQ e a API.

```bash
# Apenas dependências
docker compose up -d mongodb rabbitmq

# Stack completa
docker compose up -d --build
```

Para a API em container, configure as variáveis de conexão apontando para os nomes de serviço Docker (`mongodb`, `rabbitmq`):

```text
MongoDb__ConnectionString=mongodb://mongodb:27017
RabbitMq__HostName=rabbitmq
```

## Fluxo de branches

- `dev`: implementação;
- `tests`: validação e CI;
- `main`: versão aprovada.

Toda alteração deve passar por `tests` antes de chegar à `main`.
