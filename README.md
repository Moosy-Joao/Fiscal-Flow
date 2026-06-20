# Fiscal-Flow

SaaS multi-tenant para recebimento e processamento de documentos fiscais eletrônicos.

## Stack implementada

- ASP.NET Core (API)
- MongoDB (persistência principal + Hangfire storage)
- RabbitMQ (fila de ingestão)
- Hangfire (processamento assíncrono)
- OpenTelemetry (traces/métricas) + health check
- Docker + docker-compose

## Como executar

```bash
dotnet restore FiscalFlow.slnx
dotnet run --project /home/runner/work/Fiscal-Flow/Fiscal-Flow/src/FiscalFlow.Api/FiscalFlow.Api.csproj
```

Ou com containers:

```bash
docker compose up --build
```

## Endpoint principal

`POST /api/fiscal-documents`

Headers obrigatórios:
- `X-Tenant-Id`
- `Idempotency-Key`

Body (JSON):

```json
{
  "externalDocumentId": "NFE-123",
  "payload": {
    "numero": "123",
    "valor": 120.50
  }
}
```

Resposta: `202 Accepted` com `documentId`, `duplicate` e `tenantId`.
