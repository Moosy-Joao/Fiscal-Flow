# Identificação das requisições (Correlation ID)

O FiscalFlow usa correlation ID para rastrear uma operação do cliente até os logs, traces e mensagens assíncronas.

## Cabeçalho HTTP

| Cabeçalho | Direção | Descrição |
|---|---|---|
| `X-Correlation-ID` | Request (opcional) | Identificador enviado pelo cliente |
| `X-Correlation-ID` | Response (sempre) | Identificador da operação |

## Comportamento

1. se o cliente envia `X-Correlation-ID` válido (não vazio, até 128 caracteres), esse valor é reutilizado;
2. caso contrário, a API gera um GUID no formato de 32 caracteres hexadecimais;
3. o valor é atribuído a `HttpContext.TraceIdentifier`;
4. o cabeçalho é incluído em todas as respostas;
5. respostas de erro incluem o mesmo valor no campo `correlationId` do `ProblemDetails`.

## Logs

O middleware de tenant registra o correlation ID no escopo estruturado:

```text
CorrelationId = <valor>
```

Isso permite filtrar logs de uma requisição específica em ferramentas de observabilidade.

## Tracing e mensageria

- o correlation ID é adicionado como tag e baggage no span OpenTelemetry (`correlation.id`);
- mensagens RabbitMQ propagam o correlation ID junto com `traceparent` e `tracestate`;
- o consumidor continua o trace da requisição original.

## Exemplo

Requisição:

```http
POST /api/fiscal-documents HTTP/1.1
X-Tenant-Id: empresa-a
X-Correlation-ID: pedido-2026-001
Content-Type: application/json
```

Resposta:

```http
HTTP/1.1 201 Created
X-Correlation-ID: pedido-2026-001
```

## Relacionado

- [`OBSERVABILITY.md`](OBSERVABILITY.md) — logs, métricas e tracing;
- [`ERROR_RESPONSES.md`](ERROR_RESPONSES.md) — correlation ID em erros.
