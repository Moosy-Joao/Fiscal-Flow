# Configuração por ambiente

O ASP.NET Core converte `:` em `__` nos nomes das variáveis de ambiente. Por exemplo, `MongoDb:DatabaseName` pode ser definido como `MongoDb__DatabaseName`.

## Aplicação

- `ASPNETCORE_ENVIRONMENT`: ambiente da aplicação.
- `ASPNETCORE_URLS`: endereços HTTP escutados.
- `FiscalDocumentUpload__MaxFileSizeBytes`: limite do arquivo XML.

## MongoDB

- `MongoDb__ConnectionString`: endereço do serviço.
- `MongoDb__DatabaseName`: banco principal.
- `MongoDb__InitializeIndexes`: criação dos índices na inicialização.

## RabbitMQ

A seção `RabbitMq` controla host, porta, virtual host, filas, exchanges, routing keys, intervalo de repetição e máximo de tentativas.

## Hangfire

A seção `BackgroundJobs` controla ativação, banco, prefixo das coleções, workers, agendas, lotes, limite de tentativas e tempo máximo de processamento.

## Observabilidade

- `Observability__ServiceName`: nome do serviço exportado.
- `Observability__ServiceVersion`: versão informada ao coletor.
- `Observability__OtlpEnabled`: habilita exportação OTLP de logs, métricas e traces.
- `Observability__OtlpEndpoint`: endereço absoluto do coletor OTLP.

A exportação permanece desabilitada quando `OtlpEnabled` é falso. Health checks e logs estruturados continuam ativos independentemente do coletor.

## Segurança

A configuração de segurança será adicionada na próxima etapa do projeto. Ela abrangerá autenticação JWT Bearer, validação de issuer e audience, autorização, claim de tenant e rate limiting.

Nenhum segredo deverá ser versionado em `appsettings.json`. Em desenvolvimento, use user-secrets ou variáveis locais. Em hospedagem, utilize o mecanismo de configuração protegida oferecido pela plataforma.

## Fluxo de branches

- `dev`: implementação;
- `tests`: validação e CI;
- `main`: versão aprovada.

Toda alteração deve passar por `tests` antes de chegar à `main`.
