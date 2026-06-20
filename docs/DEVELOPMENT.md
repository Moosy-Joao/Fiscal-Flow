# Guia de desenvolvimento

O FiscalFlow é desenvolvido em pequenas entregas, cada uma isolada em sua própria branch.

## Processo

1. Atualizar a branch principal.
2. Criar uma branch de trabalho.
3. Implementar uma mudança coesa.
4. Executar build e testes.
5. Abrir Pull Request.
6. Mesclar após validação.

## Commits

Formato recomendado:

```text
<tipo>(<escopo opcional>): <descrição>
```

Tipos principais:

- `feat`;
- `fix`;
- `docs`;
- `refactor`;
- `test`;
- `chore`;
- `ci`.

## Validação

```text
dotnet restore FiscalFlow.slnx
dotnet build FiscalFlow.slnx
dotnet test FiscalFlow.slnx
```

A documentação deve acompanhar mudanças de comportamento e arquitetura.
