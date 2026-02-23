# Case 6 - Data Verification by Time Window (SQL Server)

## Script structure
- `cases/case-06-price-period-validation/scripts/01_create_schema.sql`
- `cases/case-06-price-period-validation/scripts/02_seed_case_data.sql`
- `cases/case-06-price-period-validation/scripts/03_create_procedure.sql`
- `cases/case-06-price-period-validation/scripts/04_run_examples.sql`

## Execution order
1. Run `01_create_schema.sql`.
2. Run `02_seed_case_data.sql`.
3. Run `03_create_procedure.sql`.
4. Run `04_run_examples.sql`.

## Adopted rules
- Language: T-SQL (SQL Server).
- Until 10:00: consider prices from the last 1 previous day.
- Until 13:00: consider prices from the last 5 previous days.
- After 13:00: consider prices from the last 30 previous days.
- If no valid price is found in the window: return `price = 0`, `status = Nao Ok`, and an error message.

## Explicit assumptions
- The lookup only considers previous days relative to the query day (same-day prices are excluded).
- "Latest price" means the most recent `DataReferencia` (`ORDER BY DataReferencia DESC`) within the allowed window.
- The procedure accepts optional `@DataHoraAtual` for deterministic tests; if omitted, it uses `SYSDATETIME()`.

## Procedure signature
- `dbo.sp_ObterPrecoPorPeriodo(@Produto, @DataHoraAtual = NULL)`

## Output columns
- `produto`
- `preco`
- `data_referencia`
- `status` (`Ok` or `Nao Ok`)
- `mensagem`
