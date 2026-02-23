/*
Case 5 - Script 04
Execucoes de exemplo.
*/

SET NOCOUNT ON;
GO

-- Caso do enunciado: deve retornar "Nao Ok"
EXEC dbo.sp_ValidarNovaDependencia
    @Investidor = 'Produto 4',
    @ProdutoComprado = 'Produto 1';
GO

-- Caso sem ciclo: deve retornar "Ok"
EXEC dbo.sp_ValidarNovaDependencia
    @Investidor = 'Produto 5',
    @ProdutoComprado = 'Produto 6';
GO
