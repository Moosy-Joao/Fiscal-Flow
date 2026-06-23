# Respostas de erro

A API converte falhas para o formato [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) (`ProblemDetails`) e inclui o correlation ID da requisição no campo `correlationId`.

## Formato padrão

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Requisição inválida",
  "detail": "Descrição do problema.",
  "status": 400,
  "instance": "/api/fiscal-documents",
  "correlationId": "a1b2c3d4e5f6789012345678901234ab"
}
```

O cabeçalho `X-Correlation-ID` na resposta contém o mesmo identificador.

## Códigos HTTP

| Código | Situação | Origem comum |
|---|---|---|
| `400 Bad Request` | Entrada inválida | Validação de modelo, tenant ausente (modo anônimo), XML inválido |
| `401 Unauthorized` | Autenticação necessária | Token JWT ausente, expirado ou com assinatura incorreta |
| `403 Forbidden` | Acesso negado | Token válido sem claim `tenant_id` |
| `404 Not Found` | Recurso não encontrado | Documento inexistente ou de outro tenant |
| `409 Conflict` | Conflito de operação | Transição de status inválida |
| `413 Payload Too Large` | Arquivo muito grande | Upload de XML acima do limite |
| `429 Too Many Requests` | Rate limit excedido | Limite de requisições por janela |
| `500 Internal Server Error` | Falha inesperada | Erro não tratado no servidor |
| `503 Service Unavailable` | Dependência indisponível | Readiness check com MongoDB ou RabbitMQ fora |

## Erros de tenant

| Modo | Situação | Status | Título |
|---|---|---|---|
| Anônimo | Cabeçalho `X-Tenant-Id` ausente | `400` | Tenant não informado |
| Autenticado | Claim `tenant_id` ausente | `403` | Tenant ausente na identidade |
| Qualquer | Tenant com caracteres inválidos | `400` | Validação do identificador |

## Erros de segurança

Com `Security:Enabled=true`:

- `401 Unauthorized` — token ausente ou inválido;
- `403 Forbidden` — token sem claims obrigatórias;
- `429 Too Many Requests` — limite excedido, com cabeçalho `Retry-After` quando disponível.

Detalhes em [`SECURITY.md`](SECURITY.md).

## Erros de validação

Validações de modelo (`ValidationProblem`) retornam `400 Bad Request` com estrutura estendida do ASP.NET Core, incluindo erros por campo.

Exemplo de upload inválido:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "File": ["O arquivo deve possuir extensão .xml."]
  }
}
```

## Erros internos

Detalhes de falhas inesperadas (`500`) permanecem apenas nos logs estruturados do servidor. A resposta ao cliente contém mensagem genérica para evitar vazamento de informação interna.

## Handler global

Exceções não capturadas nos controllers são tratadas por `ApiExceptionHandler`, que mapeia:

- `ArgumentException` / `InvalidDataException` → `400`;
- `KeyNotFoundException` → `404`;
- `InvalidOperationException` → `409`;
- `UnauthorizedAccessException` → `403`;
- demais → `500`.
