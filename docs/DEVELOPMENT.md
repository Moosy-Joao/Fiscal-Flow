# Guia de desenvolvimento

O FiscalFlow evolui em entregas incrementais, validadas na branch `tests` antes de chegar à `main`.

## Fluxo de branches

O repositório utiliza três branches permanentes:

```text
dev   → implementação e correções
tests → validação integrada e CI
main  → versão aprovada e estável
```

Processo:

1. implementar na `dev`;
2. sincronizar `tests` com a `dev`;
3. executar restore, build e testes na `tests`;
4. corrigir falhas na `dev` e repetir a validação;
5. promover `tests` para `main` somente quando tudo estiver aprovado;
6. sincronizar novamente `dev` e `tests` com a `main`.

Nenhuma branch adicional é necessária para o fluxo atual.

## Commits

Formato recomendado:

```text
<tipo>(<escopo opcional>): <descrição>
```

Tipos principais:

- `feat` — nova funcionalidade;
- `fix` — correção de bug;
- `docs` — documentação;
- `refactor` — refatoração sem mudança de comportamento;
- `test` — testes;
- `chore` — tarefas de manutenção;
- `ci` — pipeline e integração contínua.

## Validação local

```bash
dotnet restore FiscalFlow.slnx
dotnet build FiscalFlow.slnx
dotnet test FiscalFlow.slnx
```

### Dependências para testes completos

Alguns testes de integração usam `WebApplicationFactory` com MongoDB em memória ou repositório fake. Testes que dependem de RabbitMQ real exigem o broker local:

```bash
docker compose up -d mongodb rabbitmq
```

### Ambientes de teste

| Ambiente | Projeto | Uso |
|---|---|---|
| `Testing` | IntegrationTests | Testes gerais (RabbitMQ/Hangfire desabilitados) |
| `SecurityTesting` | IntegrationTests | Autenticação, autorização e rate limiting |

## Estrutura de projetos

```text
src/
  FiscalFlow.Api           → HTTP, middleware, jobs, consumidor
  FiscalFlow.Application   → casos de uso
  FiscalFlow.Domain        → entidades e regras
  FiscalFlow.Infrastructure → MongoDB, RabbitMQ
  FiscalFlow.E2ETests      → testes ponta a ponta (fora da solution principal)

tests/
  FiscalFlow.UnitTests
  FiscalFlow.IntegrationTests
```

## Convenções

- casos de uso na camada Application, um serviço por operação;
- contratos de persistência e mensageria como interfaces na Application;
- implementações concretas apenas na Infrastructure;
- regras de negócio na Domain, sem dependência de frameworks;
- configuração por seções tipadas (`*Options`) e feature extensions na API;
- respostas de erro via `ProblemDetails` com `correlationId`.

## Documentação

A documentação deve acompanhar mudanças de comportamento, endpoints, configuração e arquitetura. Arquivos principais:

- `README.md` — visão geral;
- `docs/API.md` — referência da API;
- `docs/ARCHITECTURE.md` — arquitetura;
- `docs/CONFIGURATION.md` — variáveis de ambiente;
- `docs/ROADMAP.md` — progresso e próximas etapas.

## Executar a API localmente

```bash
docker compose up -d mongodb rabbitmq
dotnet run --project src/FiscalFlow.Api/FiscalFlow.Api.csproj --launch-profile http
```

A API escuta em `http://localhost:5298`.
