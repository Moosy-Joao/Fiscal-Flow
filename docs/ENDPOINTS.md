# Catálogo de endpoints

Referência rápida dos endpoints HTTP expostos pela API. Para payloads, respostas e exemplos completos, consulte [`API.md`](API.md).

## Endpoints públicos

| Operação | Método | Caminho | Autenticação |
|---|---|---|---|
| Saúde básica | `GET` | `/api/health` | Não |
| Liveness | `GET` | `/health/live` | Não |
| Readiness | `GET` | `/health/ready` | Não |
| OpenAPI (Development) | `GET` | `/openapi/v1.json` | Não |

## Endpoints fiscais

Todos exigem tenant (`X-Tenant-Id` ou claim `tenant_id`). Com `Security:Enabled=true`, também exigem JWT Bearer.

| Operação | Método | Caminho |
|---|---|---|
| Criar documento (JSON) | `POST` | `/api/fiscal-documents` |
| Enviar XML | `POST` | `/api/fiscal-documents/upload` |
| Listar documentos | `GET` | `/api/fiscal-documents` |
| Consultar por ID | `GET` | `/api/fiscal-documents/{id}` |
| Atualizar status | `PATCH` | `/api/fiscal-documents/{id}/status` |

## Cabeçalhos comuns

| Cabeçalho | Obrigatório | Descrição |
|---|---|---|
| `X-Tenant-Id` | Sim (modo anônimo) | Identificador do tenant |
| `Authorization` | Sim (segurança habilitada) | Token JWT Bearer |
| `X-Correlation-ID` | Não | Rastreamento da requisição |
| `Content-Type` | Sim (POST/PATCH) | `application/json` ou `multipart/form-data` |

## Códigos de resposta por operação

### Criar documento (`POST /api/fiscal-documents` e `/upload`)

| Código | Situação |
|---|---|
| `201 Created` | Novo documento criado |
| `200 OK` | Documento idempotente já existente |
| `400 Bad Request` | Entrada inválida ou tenant ausente |
| `401 Unauthorized` | Token ausente ou inválido |
| `403 Forbidden` | Token sem claim `tenant_id` |
| `413 Payload Too Large` | Arquivo XML acima do limite (upload) |
| `429 Too Many Requests` | Rate limit excedido |

### Listar / consultar

| Código | Situação |
|---|---|
| `200 OK` | Sucesso |
| `400 Bad Request` | Parâmetros ou tenant inválidos |
| `404 Not Found` | Documento inexistente ou de outro tenant |

### Atualizar status

| Código | Situação |
|---|---|
| `200 OK` | Status atualizado |
| `400 Bad Request` | Status ou dados inválidos |
| `404 Not Found` | Documento inexistente |
| `409 Conflict` | Transição não permitida |

## Coleção de requisições

Arquivos de exemplo para reprodução manual estão em [`requests/`](../requests/).
