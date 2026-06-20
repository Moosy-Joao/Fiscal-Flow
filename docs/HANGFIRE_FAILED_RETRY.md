# Reprocessamento de documentos com falha

O Hangfire executa o job `retry-failed-documents` conforme `FailedRetryCron`. A rotina captura documentos em `Failed` de forma atômica, respeita `FailedBatchSize` e interrompe novas tentativas quando `MaximumFailedAttempts` é atingido.
