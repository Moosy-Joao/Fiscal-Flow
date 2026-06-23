# Referência da API

Base local (perfil `http`):

```text
http://localhost:5298
```

Base Docker Compose:

```text
http://localhost:8080
```

## Tenant obrigatório

Todos os endpoints de documentos exigem um tenant válido:

- **Sem autenticação** (padrão local): cabeçalho `X-Tenant-Id`;
- **Com autenticação** (`Security:Enabled=true`): claim `tenant_id` no JWT Bearer.

Endpoints de saúde (`/api/health`, `/health/live`, `/health/ready`) não exigem tenant.

## Correlation ID

Todas as respostas incluem `X-Correlation-ID`. O cliente pode enviar o mesmo cabeçalho na requisição para rastrear a operação nos logs e no tracing. Detalhes em [`REQUEST_ID.md`](REQUEST_ID.md).

## Autenticação (opcional)

Com `Security:Enabled=true`, os endpoints fiscais exigem:

```http
Authorization: Bearer <token-jwt>
```

O token deve conter as claims `sub` e `tenant_id`. Detalhes em [`SECURITY.md`](SECURITY.md).

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

### `GET /health/live`

Liveness probe. Confirma que o processo está em execução. Não verifica dependências externas.

### `GET /health/ready`

Readiness probe. Verifica MongoDB e, quando habilitado, RabbitMQ. Retorna `503` se alguma dependência obrigatória estiver indisponível.

## Criar documento com JSON

### `POST /api/fiscal-documents`

Headers:

```http
Content-Type: application/json
X-Tenant-Id: empresa-a
```

Body:

```json
{
  "externalDocumentId": "NFE-123",
  "xmlContent": "<nfeProc>...</nfeProc>"
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

Enquanto um documento existente permanecer em `Received`, uma nova requisição também tenta republicar sua mensagem no RabbitMQ. Isso permite recuperar documentos persistidos durante uma indisponibilidade temporária do broker sem criar duplicidades no MongoDB.

## Enviar arquivo XML

### `POST /api/fiscal-documents/upload`

O endpoint recebe `multipart/form-data` e aplica o mesmo fluxo de persistência, idempotência e processamento assíncrono da criação com JSON.

Campos do formulário:

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `externalDocumentId` | texto | sim | Identificador do documento no sistema de origem |
| `file` | arquivo | sim | Arquivo fiscal com extensão `.xml` |

Exemplo com `curl`:

```bash
curl -X POST http://localhost:5298/api/fiscal-documents/upload \
  -H "X-Tenant-Id: empresa-a" \
  -F "externalDocumentId=NFE-123" \
  -F "file=@nota-fiscal.xml;type=application/xml"
```

Validações realizadas antes da persistência:

- extensão `.xml`, sem diferenciar letras maiúsculas e minúsculas;
- arquivo não vazio;
- limite padrão de 2 MB;
- limite aplicado ao tamanho declarado e aos bytes realmente lidos;
- XML bem formado;
- DTD e resolução de entidades externas desabilitadas;
- presença da estrutura fiscal esperada, incluindo `infNFe` e os campos obrigatórios processados pela aplicação.

O limite pode ser alterado em configuração:

```json
{
  "FiscalDocumentUpload": {
    "MaxFileSizeBytes": 2097152
  }
}
```

Resultados:

- `201 Created`: arquivo validado e novo documento criado;
- `200 OK`: documento idempotente já existente;
- `400 Bad Request`: extensão, conteúdo ou estrutura fiscal inválida;
- `413 Payload Too Large`: arquivo acima do limite permitido.

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
      "status": "Processed",
      "receivedAtUtc": "2026-06-20T00:00:00+00:00",
      "processedAtUtc": "2026-06-20T00:00:05+00:00",
      "failureReason": null,
      "fiscalData": {
        "accessKey": "35260612345678901234567890123456789012345678",
        "issuerDocument": "12345678000199",
        "issuerName": "Empresa Emissora LTDA",
        "recipientDocument": "98765432000188",
        "recipientName": "Empresa Destinatária LTDA",
        "totalValue": 1500.00,
        "issuedAt": "2026-06-19T14:30:00+00:00"
      }
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1
}
```

O campo `fiscalData` é preenchido após processamento bem-sucedido do XML.

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
- `400 Bad Request`: tenant ausente (modo anônimo);
- `403 Forbidden`: token autenticado sem claim `tenant_id`.

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

> **Nota:** em operação normal, as transições de `Received` → `Processing` → `Processed`/`Failed` são executadas automaticamente pelo consumidor RabbitMQ. A atualização manual permanece disponível para testes e integrações controladas.

## Regras de status

```text
Received → Processing
Received → Failed
Processing → Processed
Processing → Failed
Failed → Processing
```

`Processed` é terminal e não retorna para estados anteriores.

## Erros comuns

| Situação | Status |
|---|---|
| Tenant ausente (anônimo) | `400 Bad Request` |
| Token sem `tenant_id` | `403 Forbidden` |
| Token ausente ou inválido (segurança habilitada) | `401 Unauthorized` |
| Limite de requisições excedido | `429 Too Many Requests` |
| Documento de outro tenant | `404 Not Found` |
| Transição inválida | `409 Conflict` |

Respostas de erro seguem `ProblemDetails` com `correlationId`. Detalhes em [`ERROR_RESPONSES.md`](ERROR_RESPONSES.md).

## OpenAPI

Em ambiente de desenvolvimento, o documento OpenAPI é exposto em `/openapi/v1.json`. A interface visual do Swagger ainda pode ser adicionada em uma etapa futura.

## Coleção de requisições

Exemplos prontos para reprodução estão na pasta [`requests/`](../requests/).
