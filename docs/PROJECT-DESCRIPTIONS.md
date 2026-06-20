# Descrições profissionais do FiscalFlow

## GitHub

API SaaS multi-tenant para recebimento e processamento de documentos fiscais, construída com C#/.NET 10, MongoDB, idempotência, testes automatizados e arquitetura em camadas.

## Portfólio

O FiscalFlow é um projeto de backend que simula uma plataforma SaaS de documentos fiscais eletrônicos. A solução utiliza C# e .NET 10, arquitetura em camadas e MongoDB, com foco em isolamento multi-tenant, idempotência, consistência sob concorrência, paginação, índices e testes automatizados.

A evolução planejada inclui RabbitMQ, consumidor assíncrono, Hangfire, XML fiscal, autenticação e observabilidade.

## LinkedIn — estado atual

Desenvolvimento de uma API SaaS multi-tenant para recebimento e gerenciamento de documentos fiscais eletrônicos utilizando C# e .NET 10.

Foram implementados arquitetura em camadas, persistência com MongoDB, criação idempotente, proteção contra duplicidade por índice composto, isolamento por tenant, filtros, paginação, consulta e atualização de status, testes automatizados e CI com GitHub Actions.

## LinkedIn — visão final

Desenvolvimento de uma plataforma backend SaaS multi-tenant para ingestão e processamento assíncrono de documentos fiscais. A arquitetura final contempla MongoDB, RabbitMQ, Hangfire, retry, dead-letter queue, importação segura de XML, autenticação e OpenTelemetry.

## Currículo

### FiscalFlow — API SaaS multi-tenant de documentos fiscais

Projeto de backend desenvolvido com C# e .NET 10. Aplicação de arquitetura em camadas, API REST, MongoDB, isolamento multi-tenant, idempotência, índices compostos, paginação, filtros, regras de status, testes unitários, testes de integração e GitHub Actions.

## Destaques técnicos

- domínio independente de infraestrutura;
- persistência NoSQL com MongoDB;
- idempotência por tenant e identificador externo;
- proteção contra concorrência com índice único;
- isolamento de dados multi-tenant;
- paginação e filtros;
- testes automatizados;
- desenvolvimento com branches e Pull Requests.

## Tecnologias atuais

```text
C# · .NET 10 · ASP.NET Core · MongoDB · Docker Compose
xUnit · OpenAPI · Git · GitHub Actions
```

## Tecnologias planejadas

```text
RabbitMQ · Hangfire · JWT · OpenTelemetry · XML fiscal · Docker
```
