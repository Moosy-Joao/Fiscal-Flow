# Detecção de processamento com tempo excedido

O Hangfire executa o job `detect-timed-out-processing` conforme a expressão cron configurada em `BackgroundJobs:TimedOutProcessingCron` (padrão: a cada 5 minutos).

## Comportamento

1. o job consulta documentos em status `Processing` cujo tempo desde a última atualização excede `ProcessingTimeoutMinutes` (padrão: 15 minutos);
2. cada documento encontrado é marcado como `Failed` com motivo indicando timeout;
3. o processamento ocorre em lotes limitados por `TimedOutProcessingBatchSize` (padrão: 20);
4. documentos marcados como `Failed` ficam elegíveis para o job `retry-failed-documents`.

## Configuração

| Chave | Padrão | Descrição |
|---|---|---|
| `BackgroundJobs__Enabled` | `true` (Development) | Habilita jobs Hangfire |
| `BackgroundJobs__TimedOutProcessingCron` | `*/5 * * * *` | Agendamento do job |
| `BackgroundJobs__TimedOutProcessingBatchSize` | `20` | Documentos por execução |
| `BackgroundJobs__ProcessingTimeoutMinutes` | `15` | Tempo máximo em `Processing` |

## Job complementar: reprocessamento de falhas

O job `retry-failed-documents` reprocessa documentos em `Failed` que ainda não atingiram `MaximumFailedAttempts`:

| Chave | Padrão | Descrição |
|---|---|---|
| `BackgroundJobs__FailedRetryCron` | `*/5 * * * *` | Agendamento do job |
| `BackgroundJobs__FailedBatchSize` | `20` | Documentos por execução |
| `BackgroundJobs__MaximumFailedAttempts` | `3` | Tentativas máximas de reprocessamento |

## Fluxo completo

```text
Documento em Processing
  → timeout excedido
  → detect-timed-out-processing marca como Failed
  → retry-failed-documents prepara para reprocessamento
  → documento volta para Received
  → mensagem republicada / consumidor reprocessa
```

## Pré-requisitos

- `BackgroundJobs:Enabled=true`;
- MongoDB acessível (storage Hangfire e persistência de documentos);
- índices MongoDB criados.

Em ambientes de teste (`Testing`, `SecurityTesting`), os jobs ficam desabilitados por configuração.

## Pendências

- exposição e proteção do dashboard Hangfire;
- rotina de limpeza de jobs antigos;
- testes dedicados das rotinas recorrentes.

Consulte [`ROADMAP.md`](ROADMAP.md) para o progresso.
