# Rotina de manutenção

O job `cleanup-old-documents` executa diariamente às 03:00. Ele remove em lotes de até 100 registros documentos com mais de 90 dias que ainda estejam em estados finais (`Processed` ou `Failed`). Documentos em `Received` ou `Processing` não são elegíveis.

Os valores podem ser alterados por `BackgroundJobs:CleanupCron`, `BackgroundJobs:DocumentRetentionDays` e `BackgroundJobs:CleanupBatchSize`.
