# Catálogo de endpoints

| Operação | Método | Caminho |
|---|---|---|
| Saúde | GET | `/api/health` |
| Criar documento | POST | `/api/fiscal-documents` |
| Enviar XML | POST | `/api/fiscal-documents/upload` |
| Listar documentos | GET | `/api/fiscal-documents` |
| Consultar por ID | GET | `/api/fiscal-documents/{id}` |
| Atualizar status | PATCH | `/api/fiscal-documents/{id}/status` |

Os endpoints de documentos usam o cabeçalho `X-Tenant-Id`. Consulte `docs/API.md` para os payloads e respostas completos.
