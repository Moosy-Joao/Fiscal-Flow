# Observabilidade

O FiscalFlow instrumenta logs, métricas e tracing com OpenTelemetry, com exportação OTLP opcional.

## Logs

A aplicação escreve logs estruturados e inclui scopes com `TraceId`, `SpanId`, correlation ID e tenant quando disponíveis.

Campos comuns nos scopes:

- `CorrelationId` — identificador da requisição;
- `TenantId` — tenant da operação.

XML completo e dados fiscais sensíveis não devem ser adicionados aos logs.

## Correlation ID

A API aceita o cabeçalho `X-Correlation-ID`. Quando o cliente não informa, a aplicação gera um valor. O identificador é:

- devolvido no cabeçalho de resposta;
- incluído em respostas de erro (`correlationId` no `ProblemDetails`);
- propagado para mensagens RabbitMQ;
- registrado nos logs estruturados.

Detalhes em [`REQUEST_ID.md`](REQUEST_ID.md).

## Tracing distribuído

O OpenTelemetry instrumenta:

- requisições ASP.NET Core;
- chamadas HTTP de saída;
- spans personalizados de publicação e consumo no RabbitMQ.

Os headers AMQP preservam `traceparent`, `tracestate` e correlation ID, ligando o processamento assíncrono à requisição original que criou o documento.

## Métricas

O meter `FiscalFlow.Documents` publica:

| Métrica | Descrição |
|---|---|
| `fiscalflow.documents.received` | Documentos recebidos |
| `fiscalflow.documents.processed` | Documentos processados com sucesso |
| `fiscalflow.documents.failed` | Documentos com falha |
| `fiscalflow.documents.dead_lettered` | Mensagens encaminhadas à DLQ |
| `fiscalflow.documents.processing.duration` | Duração do processamento (ms) |

As métricas são agregadas e não usam tenant, documento ou chave fiscal como dimensões, evitando cardinalidade excessiva.

## Health checks

| Endpoint | Tipo | Verifica |
|---|---|---|
| `/health/live` | Liveness | Processo em execução |
| `/health/ready` | Readiness | MongoDB e RabbitMQ (quando habilitado) |
| `/api/health` | Compatibilidade | Estado básico da aplicação |

O endpoint de readiness retorna status não saudável (`503`) quando uma dependência obrigatória está indisponível. Endpoints de saúde são anônimos e isentos de rate limiting.

## Exportação OTLP

A exportação de logs, métricas e traces fica desabilitada por padrão. Para habilitar:

```text
Observability__OtlpEnabled=true
Observability__OtlpEndpoint=<endpoint-do-coletor>
Observability__ServiceName=FiscalFlow
Observability__ServiceVersion=1.0.0
```

O endpoint deve apontar para um coletor compatível com OTLP (por exemplo, OpenTelemetry Collector), que pode encaminhar os sinais para ferramentas de logs, métricas, tracing e dashboards.

## Configuração

Variáveis completas em [`CONFIGURATION.md`](CONFIGURATION.md#observabilidade).
