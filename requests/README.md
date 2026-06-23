# Catálogo de endpoints

Referência rápida dos endpoints HTTP. Para payloads e exemplos completos, consulte [`docs/API.md`](../docs/API.md) e [`docs/ENDPOINTS.md`](../docs/ENDPOINTS.md).

| Operação | Método | Caminho |
|---|---|---|
| Saúde básica | `GET` | `/api/health` |
| Liveness | `GET` | `/health/live` |
| Readiness | `GET` | `/health/ready` |
| Criar documento (JSON) | `POST` | `/api/fiscal-documents` |
| Enviar XML | `POST` | `/api/fiscal-documents/upload` |
| Listar documentos | `GET` | `/api/fiscal-documents` |
| Consultar por ID | `GET` | `/api/fiscal-documents/{id}` |
| Atualizar status | `PATCH` | `/api/fiscal-documents/{id}/status` |

## Cabeçalhos

- `X-Tenant-Id` — obrigatório nos endpoints fiscais (modo local sem autenticação);
- `Authorization: Bearer <token>` — obrigatório quando `Security:Enabled=true`;
- `X-Correlation-ID` — opcional, para rastreamento.

Base local: `http://localhost:5298`
