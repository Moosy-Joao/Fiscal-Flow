# Roadmap do FiscalFlow

## Princípio de evolução

O projeto é desenvolvido em incrementos pequenos. Cada funcionalidade passa por:

```text
branch → implementação → testes → Pull Request → merge → limpeza da branch
```

## Fase 1 — Fundação

- [x] solução organizada em projetos;
- [x] separação entre API, Application, Domain e Infrastructure;
- [x] testes unitários;
- [x] testes de integração;
- [x] GitHub Actions para restore, build, testes e cobertura;
- [x] Docker Compose de desenvolvimento.

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

- [x] cabeçalho `X-Tenant-Id`;
- [x] middleware de tenant;
- [x] contexto por requisição;
- [x] isolamento na criação;
- [x] isolamento na consulta;
- [x] isolamento na listagem;
- [x] isolamento na atualização;
- [x] testes de acesso cruzado.

## Fase 5 — Idempotência

- [x] busca por `tenantId + externalDocumentId`;
- [x] retorno do documento existente;
- [x] `201 Created` para nova criação;
- [x] `200 OK` para repetição;
- [x] proteção concorrente por índice único;
- [x] testes unitários;
- [x] validação manual no MongoDB.

## Fase 6 — RabbitMQ

- [ ] adicionar RabbitMQ ao Docker Compose;
- [ ] criar opções de configuração;
- [ ] criar contrato de mensagem;
- [ ] publicar mensagem após persistência;
- [ ] criar consumidor assíncrono;
- [ ] atualizar status automaticamente;
- [ ] implementar retry;
- [ ] implementar dead-letter queue;
- [ ] testar duplicidade de mensagens;
- [ ] testar indisponibilidade do broker.

### Definição de concluído

- API recebe e persiste rapidamente;
- processamento acontece fora da requisição HTTP;
- mensagens repetidas não causam processamento duplicado;
- falhas irrecuperáveis são encaminhadas para uma fila própria.

## Fase 7 — Hangfire

- [ ] configurar storage;
- [ ] agendar reprocessamento de falhas;
- [ ] detectar documentos presos em `Processing`;
- [ ] criar rotina de limpeza;
- [ ] proteger dashboard;
- [ ] adicionar testes das rotinas.

## Fase 8 — XML fiscal

- [ ] endpoint de upload;
- [ ] limite de tamanho;
- [ ] validação de XML;
- [ ] leitura segura;
- [ ] extração de dados fiscais;
- [ ] armazenamento dos dados extraídos;
- [ ] tratamento de XML inválido;
- [ ] amostras de XML para testes;
- [ ] testes de segurança e integração.

## Fase 9 — Observabilidade

- [ ] logs estruturados;
- [ ] correlation ID;
- [ ] propagação entre API e consumidor;
- [ ] métricas de recebimento, sucesso e falha;
- [ ] tempo de processamento;
- [ ] tracing com OpenTelemetry;
- [ ] health checks de MongoDB e RabbitMQ;
- [ ] dashboards ou exportadores.

## Fase 10 — Segurança

- [ ] autenticação JWT;
- [ ] tenant obtido pelas claims;
- [ ] autorização;
- [ ] rate limiting;
- [ ] configuração de segredos;
- [ ] padronização global de erros;
- [ ] validações adicionais de entrada;
- [ ] revisão de segurança.

## Fase 11 — Entrega final

- [ ] Dockerfile da API;
- [ ] Dockerfile do consumidor;
- [ ] Docker Compose completo;
- [ ] variáveis de ambiente documentadas;
- [ ] coleção de requisições;
- [ ] diagrama final;
- [ ] testes ponta a ponta;
- [ ] deploy;
- [ ] revisão do README;
- [ ] demonstração para portfólio.

## Critérios gerais de qualidade

Uma fase só deve ser considerada concluída quando:

- o código compila sem erros;
- os testes passam;
- o comportamento foi validado manualmente quando necessário;
- a documentação foi atualizada;
- o Pull Request foi revisado e mesclado;
- a branch foi excluída local e remotamente;
- a `main` ficou sincronizada e limpa.
