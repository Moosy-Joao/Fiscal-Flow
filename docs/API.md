# Referência da API

Base local:

```text
http://localhost:5298
```

## Tenant obrigatório

Todos os endpoints de documentos exigem:

```http
X-Tenant-Id: empresa-a
```

O endpoint `/api/health` não exige tenant.

## Health

### `GET /api/health`

Resposta `200 OK`:

```json
{
  "application": "FiscalFlow",
  "status": "Healthy",
  "checkedAtUtc": "2026-06-20T00:00:00+00:00"
}
```

## Criar documento

### `POST /api/fiscal-documents`

Headers:

```http
Content-Type: application/json
X-Tenant-Id: empresa-a
```

Body:

```json
{
  "externalDocumentId": "NFE-123"
}
```

Primeira criação: `201 Created`.

```json
{
  "id": "2d218390-8fc0-44cb-9488-aa49b9187f81",
  "tenantId": "empresa-a",
  "externalDocumentId": "NFE-123",
  "status": "Received",
  "receivedAtUtc": "2026-06-20T00:00:00+00:00",
  "wasCreated": true
}
```

Repetição da mesma combinação de tenant e identificador externo: `200 OK`.

```json
{
  "id": "2d218390-8fc0-44cb-9488-aa49b9187f81",
  "tenantId": "empresa-a",
  "externalDocumentId": "NFE-123",
  "status": "Received",
  "receivedAtUtc": "2026-06-20T00:00:00+00:00",
  "wasCreated": false
}
```

## Listar documentos

### `GET /api/fiscal-documents`

Parâmetros opcionais:

| Parâmetro | Padrão | Regra |
|---|---:|---|
| `status` | vazio | `Received`, `Processing`, `Processed` ou `Failed` |
| `page` | `1` | mínimo 1 |
| `pageSize` | `10` | entre 1 e 100 |

Exemplo:

```http
GET /api/fiscal-documents?status=Received&page=1&pageSize=20
X-Tenant-Id: empresa-a
```

Resposta `200 OK`:

```json
{
  "items": [
    {
      "id": "2d218390-8fc0-44cb-9488-aa49b9187f81",
      "tenantId": "empresa-a",
      "externalDocumentId": "NFE-123",
      "status": "Received",
      "receivedAtUtc": "2026-06-20T00:00:00+00:00",
      "processedAtUtc": null,
      "failureReason": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1
}
```

## Consultar por ID

### `GET /api/fiscal-documents/{id}`

Exemplo:

```http
GET /api/fiscal-documents/2d218390-8fc0-44cb-9488-aa49b9187f81
X-Tenant-Id: empresa-a
```

Resultados:

- `200 OK`: documento encontrado para o tenant;
- `404 Not Found`: documento inexistente ou pertencente a outro tenant;
- `400 Bad Request`: tenant ausente.

## Atualizar status

### `PATCH /api/fiscal-documents/{id}/status`

Body para iniciar processamento:

```json
{
  "status": "Processing"
}
```

Body para concluir:

```json
{
  "status": "Processed"
}
```

Body para falha:

```json
{
  "status": "Failed",
  "failureReason": "XML inválido"
}
```

Resultados:

- `200 OK`: status atualizado;
- `400 Bad Request`: status ou dados inválidos;
- `404 Not Found`: documento inexistente para o tenant;
- `409 Conflict`: transição de status não permitida.

## Regras de status

```text
Received → Processing
Received → Failed
Processing → Processed
Processing → Failed
Failed → Processing
```

`Processed` é terminal e não retorna para estados anteriores.

## Erros de tenant

Sem o cabeçalho obrigatório, a API retorna `400 Bad Request` com `ProblemDetails`.

Um tenant tentando acessar ou atualizar um documento de outro tenant recebe `404 Not Found`.

## OpenAPI

Em ambiente de desenvolvimento, o documento OpenAPI é exposto pelo ASP.NET Core. A interface visual do Swagger ainda pode ser adicionada em uma etapa futura.
