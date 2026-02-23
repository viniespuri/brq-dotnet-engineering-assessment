# Case 5 - Validacao de Ciclo em Dependencias (SQL Server)

## Estrutura dos scripts
- `cases/case-05-cycle-validation/scripts/01_create_schema.sql`
- `cases/case-05-cycle-validation/scripts/02_seed_case_data.sql`
- `cases/case-05-cycle-validation/scripts/03_create_procedure.sql`
- `cases/case-05-cycle-validation/scripts/04_run_examples.sql`

## Ordem de execucao
1. Executar `01_create_schema.sql`.
2. Executar `02_seed_case_data.sql`.
3. Executar `03_create_procedure.sql`.
4. Executar `04_run_examples.sql`.

## Regras adotadas
- Linguagem: T-SQL (SQL Server).
- Inserir `A -> B` e invalido se ja existir caminho `B -> ... -> A`.
- Duplicado `A -> B` e tratado como `Nao Ok`.
- Auto-relacionamento `A -> A` e `Nao Ok`.

## Exemplo do enunciado
Dados existentes:

`Investidor -> ProdutoComprado`
- `Produto 1 -> Produto 2`
- `Produto 1 -> Produto 3`
- `Produto 2 -> Produto 4`
- `Produto 3 -> Produto 5`

Novo registro:
- `Produto 4 -> Produto 1`

Saida esperada:
- `status = Não Ok`
- mensagem com caminho de conflito (exemplo):
`Produto 1 -> Produto 2 -> Produto 4`
- ciclo fechado ao inserir:
`Produto 4 -> Produto 1 -> Produto 2 -> Produto 4`

## Logica da procedure
- Valida parametros (nulo/vazio) e limite de profundidade.
- Verifica duplicado antes da recursao.
- Executa CTE recursiva partindo de `@ProdutoComprado`.
- Usa `PathNodes` para nao repetir no ja visitado.
- Limita profundidade por parametro (`@MaxProfundidade`) e `MAXRECURSION`.
- Retorna `Ok`/`Não Ok` com mensagem explicativa.
