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

A configuração inclui host, porta, virtual host, nomes das filas, exchanges, routing keys, intervalo de repetição e máximo de tentativas. Os nomes correspondem à seção `RabbitMq` de `appsettings.Development.json`.

## Hangfire

A seção `BackgroundJobs` controla ativação, banco, prefixo das coleções, workers, agendas, lotes, limite de tentativas e tempo máximo de processamento.

## Valores sensíveis

Valores de autenticação não devem ser versionados. Em desenvolvimento, use user-secrets ou variáveis locais. Em hospedagem, utilize o mecanismo de configuração protegida oferecido pela plataforma.
