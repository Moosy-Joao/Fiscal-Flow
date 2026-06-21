# Observabilidade

## Logs

A aplicação escreve logs estruturados em JSON e inclui scopes, `TraceId`, `SpanId`, correlation ID e tenant quando disponíveis. XML completo e dados fiscais não devem ser adicionados aos logs.

## Correlation ID

A API aceita `X-Correlation-ID`. Quando o cliente não informa o cabeçalho, a aplicação gera um valor. O identificador é devolvido na resposta e propagado para o RabbitMQ.

## Tracing distribuído

O OpenTelemetry instrumenta requisições ASP.NET Core, chamadas HTTP e os spans personalizados de publicação e consumo no RabbitMQ. Os headers AMQP preservam `traceparent`, `tracestate` e correlation ID, ligando o processamento assíncrono à requisição original.

## Métricas

O meter `FiscalFlow.Documents` publica:

- `fiscalflow.documents.received`;
- `fiscalflow.documents.processed`;
- `fiscalflow.documents.failed`;
- `fiscalflow.documents.dead_lettered`;
- `fiscalflow.documents.processing.duration` em milissegundos.

As métricas são agregadas e não usam tenant, documento ou chave fiscal como dimensões.

## Health checks

- `/health/live`: confirma que o processo está executando;
- `/health/ready`: verifica MongoDB e, quando habilitado, RabbitMQ;
- `/api/health`: endpoint simples mantido para compatibilidade.

O endpoint de readiness retorna status não saudável quando uma dependência obrigatória está indisponível.

## Exportação OTLP

A exportação de logs, métricas e traces fica desabilitada por padrão. Para habilitar:

```text
Observability__OtlpEnabled=true
Observability__OtlpEndpoint=<endpoint-do-coletor>
Observability__ServiceName=FiscalFlow
Observability__ServiceVersion=1.0.0
```

O endpoint deve apontar para um coletor compatível com OTLP, que pode encaminhar os sinais para ferramentas de logs, métricas, tracing e dashboards.
