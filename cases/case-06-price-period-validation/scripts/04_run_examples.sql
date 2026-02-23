/*
Case 6 - Script 04
Execucoes de exemplo.
*/

SET NOCOUNT ON;
GO

-- Regra < 10h (1 dia anterior): encontra o preco de ontem.
EXEC dbo.sp_ObterPrecoPorPeriodo
    @Produto = 'PROD-A',
    @DataHoraAtual = '2026-02-23 09:30:00';
GO

-- Regra < 10h (1 dia anterior): sem preco no periodo, retorna Nao Ok e preco 0.
EXEC dbo.sp_ObterPrecoPorPeriodo
    @Produto = 'PROD-B',
    @DataHoraAtual = '2026-02-23 09:30:00';
GO

-- Regra entre 10h e 13h (5 dias anteriores): encontra preco de 3 dias anteriores.
EXEC dbo.sp_ObterPrecoPorPeriodo
    @Produto = 'PROD-B',
    @DataHoraAtual = '2026-02-23 11:00:00';
GO

-- Regra >= 13h (30 dias anteriores): encontra preco mais recente dentro da janela.
EXEC dbo.sp_ObterPrecoPorPeriodo
    @Produto = 'PROD-A',
    @DataHoraAtual = '2026-02-23 14:00:00';
GO

-- Produto inexistente: retorna Nao Ok e preco 0.
EXEC dbo.sp_ObterPrecoPorPeriodo
    @Produto = 'PROD-Z',
    @DataHoraAtual = '2026-02-23 14:00:00';
GO
