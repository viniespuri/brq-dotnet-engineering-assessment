# Case 5 - Dependency Cycle Validation (SQL Server)

## Script structure
- `cases/case-05-cycle-validation/scripts/01_create_schema.sql`
- `cases/case-05-cycle-validation/scripts/02_seed_case_data.sql`
- `cases/case-05-cycle-validation/scripts/03_create_procedure.sql`
- `cases/case-05-cycle-validation/scripts/04_run_examples.sql`

## Execution order
1. Run `01_create_schema.sql`.
2. Run `02_seed_case_data.sql`.
3. Run `03_create_procedure.sql`.
4. Run `04_run_examples.sql`.

## Adopted rules
- Language: T-SQL (SQL Server).
- Inserting `A -> B` is invalid if path `B -> ... -> A` already exists.
- Duplicate `A -> B` is treated as `Nao Ok`.
- Self-relationship `A -> A` is `Nao Ok`.

## Prompt example
Existing data:

`Investidor -> ProdutoComprado`
- `Produto 1 -> Produto 2`
- `Produto 1 -> Produto 3`
- `Produto 2 -> Produto 4`
- `Produto 3 -> Produto 5`

New record:
- `Produto 4 -> Produto 1`

Expected output:
- `status = Nao Ok`
- message with conflict path (example):
`Produto 1 -> Produto 2 -> Produto 4`
- cycle closed when inserting:
`Produto 4 -> Produto 1 -> Produto 2 -> Produto 4`

## Procedure logic
- Validates parameters (null/empty) and depth limit.
- Checks duplicate before recursion.
- Runs recursive CTE starting from `@ProdutoComprado`.
- Uses `PathNodes` to avoid revisiting nodes.
- Limits depth by parameter (`@MaxProfundidade`) and `MAXRECURSION`.
- Returns `Ok`/`Nao Ok` with explanatory message.
