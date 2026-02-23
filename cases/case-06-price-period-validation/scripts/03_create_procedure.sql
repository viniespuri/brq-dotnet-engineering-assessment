/*
Case 6 - Script 03
Cria procedure para buscar o ultimo preco com limite por periodo, de acordo com horario atual.

Regras:
- Ate 10h: considerar ate 1 dia anterior.
- Ate 13h: considerar ate 5 dias anteriores.
- Apos 13h: considerar ate 30 dias anteriores.
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ObterPrecoPorPeriodo
    @Produto VARCHAR(100),
    @DataHoraAtual DATETIME2(0) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Limpo o parametro para evitar erro por espacos em branco.
    DECLARE @ProdutoTrim VARCHAR(100) = LTRIM(RTRIM(@Produto));
    -- Permito sobrescrever a data/hora atual para testes deterministas.
    DECLARE @Agora DATETIME2(0) = ISNULL(@DataHoraAtual, SYSDATETIME());
    DECLARE @HoraAtual INT = DATEPART(HOUR, @Agora);
    DECLARE @DiasLimite INT;
    -- Eu uso a data base sem horario para trabalhar com "dias anteriores" de forma objetiva.
    DECLARE @DataBase DATE = CAST(@Agora AS DATE);
    DECLARE @DataInicial DATETIME2(0);
    DECLARE @DataFinalExclusiva DATETIME2(0);
    DECLARE @PrecoEncontrado DECIMAL(18, 4);
    DECLARE @DataReferenciaEncontrada DATETIME2(0);
    DECLARE @StatusNaoOk NVARCHAR(10) = N'N' + NCHAR(227) + N'o Ok';

    IF @Produto IS NULL OR @ProdutoTrim = ''
    BEGIN
        SELECT
            produto = @Produto,
            preco = CAST(0 AS DECIMAL(18, 4)),
            data_referencia = CAST(NULL AS DATETIME2(0)),
            [status] = @StatusNaoOk,
            mensagem = N'Produto deve ser informado.';
        RETURN;
    END;

    -- Aplico a regra de negocio da janela conforme a hora corrente.
    IF @HoraAtual < 10
    BEGIN
        SET @DiasLimite = 1;
    END
    ELSE IF @HoraAtual < 13
    BEGIN
        SET @DiasLimite = 5;
    END
    ELSE
    BEGIN
        SET @DiasLimite = 30;
    END;

    -- Eu considero apenas dias anteriores: inicio inclusivo e "hoje 00:00" como fim exclusivo.
    SET @DataInicial = DATEADD(DAY, -@DiasLimite, CAST(@DataBase AS DATETIME2(0)));
    SET @DataFinalExclusiva = CAST(@DataBase AS DATETIME2(0));

    -- Eu busco o ultimo preco dentro da janela valida, ordenando pela referencia mais recente.
    SELECT TOP (1)
        @PrecoEncontrado = p.Preco,
        @DataReferenciaEncontrada = p.DataReferencia
    FROM dbo.Precos p
    WHERE p.Produto = @ProdutoTrim
      AND p.DataReferencia >= @DataInicial
      AND p.DataReferencia < @DataFinalExclusiva
    ORDER BY p.DataReferencia DESC;

    IF @PrecoEncontrado IS NULL
    BEGIN
        SELECT
            produto = @ProdutoTrim,
            preco = CAST(0 AS DECIMAL(18, 4)),
            data_referencia = CAST(NULL AS DATETIME2(0)),
            [status] = @StatusNaoOk,
            mensagem = CONCAT(
                N'Preco nao encontrado para o produto no periodo permitido (ultimos ',
                @DiasLimite,
                N' dia(s) anteriores).'
            );
        RETURN;
    END;

    -- Eu retorno status Ok quando encontro um preco valido dentro das regras de horario.
    SELECT
        produto = @ProdutoTrim,
        preco = @PrecoEncontrado,
        data_referencia = @DataReferenciaEncontrada,
        [status] = N'Ok',
        mensagem = N'Preco encontrado com sucesso.';
END;
GO
