# Uso da coleção de requisições

Use os arquivos desta pasta junto com a documentação da API para reproduzir o fluxo completo da aplicação.

## Pré-requisitos

1. subir dependências:

```bash
docker compose up -d mongodb rabbitmq
```

2. iniciar a API:

```bash
dotnet run --project src/FiscalFlow.Api/FiscalFlow.Api.csproj --launch-profile http
```

## Fluxo recomendado

1. **Health** — verificar `/api/health` ou `/health/ready`;
2. **Criar documento** — `POST /api/fiscal-documents` com JSON ou `/upload` com XML;
3. **Consultar** — `GET /api/fiscal-documents/{id}` após processamento assíncrono;
4. **Listar** — `GET /api/fiscal-documents?status=Processed`;
5. **Idempotência** — repetir a criação com o mesmo `externalDocumentId` e observar `200 OK`.

## Cabeçalhos obrigatórios

```http
X-Tenant-Id: empresa-a
```

Com segurança habilitada, inclua também:

```http
Authorization: Bearer <token-jwt>
```

## Documentação

- [`docs/API.md`](../docs/API.md) — referência completa;
- [`docs/ENDPOINTS.md`](../docs/ENDPOINTS.md) — catálogo de endpoints;
- [`docs/CONFIGURATION.md`](../docs/CONFIGURATION.md) — configuração por ambiente.
