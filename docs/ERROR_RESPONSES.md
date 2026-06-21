# Respostas de erro

A API converte falhas não tratadas para o formato `ProblemDetails` e inclui o correlation ID da requisição.

- entrada inválida: HTTP 400;
- recurso não encontrado: HTTP 404;
- conflito de operação: HTTP 409;
- acesso negado: HTTP 403;
- falha inesperada: HTTP 500 com descrição genérica.

Detalhes internos de falhas inesperadas permanecem apenas nos logs estruturados do servidor.
