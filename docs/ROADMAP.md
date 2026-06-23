# Roadmap do FiscalFlow

## Fluxo de evolução

O projeto utiliza somente três branches permanentes:

```text
dev → tests → main
```

- `dev`: implementação e correções;
- `tests`: validação integrada e execução do CI;
- `main`: versão aprovada e estável.

Uma alteração só chega à `main` depois de compilar e passar por todos os testes na `tests`. Quando houver falha, a correção volta para a `dev`, é sincronizada novamente com a `tests` e validada outra vez.

## Fase 1 — Fundação

- [x] solução organizada em projetos;
- [x] separação entre API, Application, Domain e Infrastructure;
- [x] testes unitários;
- [x] testes de integração;
- [x] GitHub Actions para restore, build, testes e cobertura;
- [x] ambiente local com Docker Compose.

## Fase 2 — MongoDB

- [x] conexão da API com MongoDB;
- [x] configuração por ambiente;
- [x] modelo de persistência;
- [x] repository;
- [x] criação automática de índices;
- [x] índice único por tenant e identificador externo;
- [x] índices para listagem e status.

## Fase 3 — Operações de documentos

- [x] criação;
- [x] consulta por ID;
- [x] atualização de status;
- [x] listagem;
- [x] filtro por status;
- [x] paginação;
- [x] respostas HTTP adequadas.

## Fase 4 — Multi-tenancy

- [x] cabeçalho `X-Tenant-Id` para modo sem autenticação;
- [x] middleware de tenant;
- [x] contexto por requisição;
- [x] isolamento na criação, consulta, listagem e atualização;
- [x] suporte à claim `tenant_id` em identidades autenticadas;
- [x] validação do identificador do tenant;
- [x] testes de acesso cruzado e precedência da claim.

## Fase 5 — Idempotência

- [x] busca por `tenantId + externalDocumentId`;
- [x] retorno do documento existente;
- [x] `201 Created` para nova criação;
- [x] `200 OK` para repetição;
- [x] proteção concorrente por índice único;
- [x] republicação de mensagem para documentos em `Received`;
- [x] testes unitários;
- [x] validação manual no MongoDB.

## Fase 6 — RabbitMQ

- [x] adicionar RabbitMQ ao ambiente local;
- [x] criar opções de configuração;
- [x] criar contrato de mensagem;
- [x] publicar mensagem após persistência;
- [x] criar consumidor assíncrono;
- [x] atualizar status automaticamente;
- [x] implementar retry;
- [x] implementar dead-letter queue;
- [x] testar duplicidade de mensagens;
- [x] testar indisponibilidade do broker;
- [x] propagação de trace context via AMQP.

### Definição de concluído

- API recebe e persiste rapidamente;
- processamento acontece fora da requisição HTTP;
- mensagens repetidas não causam processamento duplicado;
- falhas irrecuperáveis são encaminhadas para uma fila própria.

## Fase 7 — Hangfire

- [x] configurar storage MongoDB;
- [x] agendar reprocessamento de falhas;
- [x] detectar documentos presos em `Processing`;
- [ ] expor dashboard com autorização;
- [ ] criar rotina de limpeza;
- [ ] ampliar testes das rotinas recorrentes.

## Fase 8 — XML fiscal

- [x] endpoint de upload;
- [x] limite de tamanho;
- [x] validação de XML;
- [x] leitura segura;
- [x] extração de dados fiscais;
- [x] armazenamento dos dados extraídos;
- [x] tratamento de XML inválido;
- [x] amostras de XML para testes;
- [x] testes de segurança e integração.

## Fase 9 — Observabilidade

- [x] logs estruturados;
- [x] correlation ID;
- [x] propagação entre API e consumidor;
- [x] métricas de recebimento, sucesso e falha;
- [x] tempo de processamento;
- [x] tracing com OpenTelemetry;
- [x] health checks de MongoDB e RabbitMQ;
- [x] exportação OTLP opcional;
- [x] endpoints `/health/live` e `/health/ready`.

## Fase 10 — Segurança

### Implementado

- [x] autenticação JWT Bearer;
- [x] validação de issuer, audience, assinatura e expiração;
- [x] autorização por política `fiscalflow-api`;
- [x] proteção dos endpoints fiscais (quando habilitada);
- [x] endpoints de saúde anônimos;
- [x] rate limiting por usuário ou endereço IP;
- [x] tenant obtido pela claim quando autenticado;
- [x] rejeição de identidade autenticada sem tenant;
- [x] padronização global de erros com `ProblemDetails`;
- [x] correlation ID nas respostas de erro;
- [x] testes de autenticação, autorização e rate limiting;
- [x] documentação de segurança.

### Pendente

- [ ] configuração segura de segredos em produção;
- [ ] proteger dashboard Hangfire;
- [ ] revisão final de segurança antes do deploy.

## Fase 11 — Entrega final

- [x] Dockerfile da API;
- [x] consumidor hospedado junto à aplicação;
- [x] Docker Compose com MongoDB, RabbitMQ e API;
- [x] variáveis de ambiente documentadas;
- [x] catálogo de endpoints;
- [x] diagrama de arquitetura e implantação;
- [x] revisão geral do README e documentação;
- [ ] configuração Docker completa para produção;
- [ ] ampliar testes ponta a ponta;
- [ ] deploy;
- [ ] demonstração para portfólio.

## Ordem atual de trabalho

1. expor e proteger dashboard Hangfire;
2. rotina de limpeza de jobs;
3. completar configuração Docker para produção;
4. ampliar testes ponta a ponta;
5. deploy quando plataforma e credenciais estiverem disponíveis.

## Critérios gerais de qualidade

Uma etapa só é considerada concluída quando:

- o código compila sem erros;
- os testes passam na branch `tests`;
- o comportamento foi validado quando necessário;
- a documentação foi atualizada;
- a alteração foi promovida para `main`;
- `dev`, `tests` e `main` ficaram sincronizadas após a entrega.
