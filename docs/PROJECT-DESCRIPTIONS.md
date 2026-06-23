# Descrições profissionais do FiscalFlow

Textos prontos para uso em GitHub, portfólio, LinkedIn e currículo.

## GitHub

API SaaS multi-tenant para recebimento e processamento assíncrono de documentos fiscais eletrônicos, construída com C#/.NET 10, MongoDB, RabbitMQ, Hangfire, idempotência, observabilidade com OpenTelemetry, segurança JWT opcional e testes automatizados.

## Portfólio — resumo

O FiscalFlow é um projeto de backend que simula uma plataforma SaaS de documentos fiscais eletrônicos. A solução utiliza C# e .NET 10, arquitetura em camadas e MongoDB, com foco em isolamento multi-tenant, idempotência, consistência sob concorrência, processamento assíncrono via RabbitMQ, jobs recorrentes com Hangfire, importação segura de XML fiscal, observabilidade e testes automatizados.

## LinkedIn — estado atual

Desenvolvimento de uma API SaaS multi-tenant para recebimento e processamento assíncrono de documentos fiscais eletrônicos utilizando C# e .NET 10.

Foram implementados arquitetura em camadas, persistência com MongoDB, criação idempotente, mensageria com RabbitMQ (retry e dead-letter queue), jobs Hangfire para reprocessamento e detecção de timeout, upload e parsing seguro de XML fiscal, observabilidade com OpenTelemetry, autenticação JWT e rate limiting opcionais, isolamento por tenant, testes automatizados e CI com GitHub Actions.

## LinkedIn — visão final

Plataforma backend SaaS multi-tenant para ingestão e processamento assíncrono de documentos fiscais, com deploy em produção, testes ponta a ponta ampliados e dashboard operacional protegido.

## Currículo

### FiscalFlow — API SaaS multi-tenant de documentos fiscais

Projeto de backend desenvolvido com C# e .NET 10. Aplicação de arquitetura em camadas, API REST, MongoDB, RabbitMQ, Hangfire, isolamento multi-tenant, idempotência, processamento assíncrono de XML fiscal, observabilidade com OpenTelemetry, segurança JWT, testes unitários, testes de integração e GitHub Actions.

## Destaques técnicos

- domínio independente de infraestrutura;
- persistência NoSQL com MongoDB e índices compostos;
- idempotência por tenant e identificador externo;
- proteção contra concorrência com índice único e captura atômica de processamento;
- isolamento de dados multi-tenant;
- mensageria com retry, dead-letter queue e propagação de trace context;
- jobs recorrentes para reprocessamento e detecção de timeout;
- parsing seguro de XML fiscal com extração de dados;
- observabilidade com logs estruturados, métricas, tracing e health checks;
- autenticação JWT e rate limiting configuráveis;
- paginação, filtros e respostas padronizadas com ProblemDetails;
- testes automatizados e pipeline de CI.

## Tecnologias

```text
C# · .NET 10 · ASP.NET Core · MongoDB · RabbitMQ · Hangfire
OpenTelemetry · JWT · Docker · Docker Compose
xUnit · OpenAPI · Git · GitHub Actions
```

## Próximos passos (para atualização futura)

```text
Deploy · testes E2E ampliados · dashboard Hangfire protegido
```
