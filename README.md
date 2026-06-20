# FiscalFlow

Projeto de estudo para aprender, passo a passo, como construir uma API SaaS multi-tenant para recebimento e processamento de documentos fiscais.

## Estado atual

A primeira fase contém somente:

- solução organizada em projetos;
- API ASP.NET Core com endpoint de saúde;
- domínio independente de banco de dados;
- testes unitários;
- testes de integração;
- GitHub Actions para build e testes;
- MongoDB disponível no Docker, ainda sem integração com a API.

RabbitMQ, Hangfire, idempotência, multi-tenancy e observabilidade serão adicionados depois, um assunto por vez.

## Executar

```bash
dotnet restore FiscalFlow.slnx
dotnet build FiscalFlow.slnx
dotnet test FiscalFlow.slnx
```

```bash
dotnet run --project src/FiscalFlow.Api/FiscalFlow.Api.csproj --launch-profile http
```

Endpoint inicial:

```text
http://localhost:5298/api/health
```

## MongoDB local

```bash
docker compose -f docker-compose.learning.yml up -d
```

O MongoDB ainda não está conectado à API. Essa será a próxima etapa prática.

## Roteiro

1. Fundação, testes e integração contínua.
2. Conceitos básicos e conexão com MongoDB.
3. CRUD de documentos fiscais.
4. Índices, filtros e paginação.
5. Multi-tenancy.
6. Idempotência e concorrência.
7. RabbitMQ com produtor e consumidor.
8. Hangfire para tarefas agendadas.
9. Importação de XML.
10. Logs, métricas e publicação.
