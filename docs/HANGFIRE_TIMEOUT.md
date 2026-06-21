# Detecção de processamento com tempo excedido

O job `detect-timed-out-processing` roda conforme `TimedOutProcessingCron`. Documentos em `Processing` há mais de `ProcessingTimeoutMinutes` são marcados como `Failed` em lotes limitados por `TimedOutProcessingBatchSize` e seguem para o job de reprocessamento existente.
