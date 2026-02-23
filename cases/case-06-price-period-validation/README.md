# Case 6 - Verificacao de Dados por Periodo (SQL Server)

## Estrutura dos scripts
- `cases/case-06-price-period-validation/scripts/01_create_schema.sql`
- `cases/case-06-price-period-validation/scripts/02_seed_case_data.sql`
- `cases/case-06-price-period-validation/scripts/03_create_procedure.sql`
- `cases/case-06-price-period-validation/scripts/04_run_examples.sql`

## Ordem de execucao
1. Executar `01_create_schema.sql`.
2. Executar `02_seed_case_data.sql`.
3. Executar `03_create_procedure.sql`.
4. Executar `04_run_examples.sql`.

## Regras adotadas
- Linguagem: T-SQL (SQL Server).
- Ate 10h: considerar precos dos ultimos 1 dia anterior.
- Ate 13h: considerar precos dos ultimos 5 dias anteriores.
- Apos 13h: considerar precos dos ultimos 30 dias anteriores.
- Se nao houver preco valido na janela: retornar `preco = 0`, `status = Nao Ok` e mensagem de erro.

## Premissas explicitas
- A busca usa somente dias anteriores ao dia atual da consulta (nao considera preco do proprio dia).
- O "ultimo preco" e o registro mais recente por `DataReferencia DESC` dentro da janela permitida.
- A procedure aceita `@DataHoraAtual` opcional para facilitar testes; se nao for informado, usa `SYSDATETIME()`.

## Assinatura da procedure
- `dbo.sp_ObterPrecoPorPeriodo(@Produto, @DataHoraAtual = NULL)`

## Retorno
- `produto`
- `preco`
- `data_referencia`
- `status` (`Ok` ou `Nao Ok`)
- `mensagem`
