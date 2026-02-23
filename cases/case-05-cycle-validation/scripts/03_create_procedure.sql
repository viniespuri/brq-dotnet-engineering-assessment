/*
Case 5 - Script 03
Cria procedure para validar se nova aresta fecha ciclo no grafo.

Regra:
Inserir A -> B e invalido se ja existir caminho de B ate A.
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ValidarNovaDependencia
    @Investidor VARCHAR(100),
    @ProdutoComprado VARCHAR(100),
    @MaxProfundidade INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StatusNaoOk NVARCHAR(10) = N'N' + NCHAR(227) + N'o Ok';
    DECLARE @Mensagem NVARCHAR(2000);
    DECLARE @Caminho NVARCHAR(2000);
    DECLARE @InvestidorTrim VARCHAR(100) = LTRIM(RTRIM(@Investidor));
    DECLARE @ProdutoTrim VARCHAR(100) = LTRIM(RTRIM(@ProdutoComprado));

    IF @Investidor IS NULL OR @ProdutoComprado IS NULL
    BEGIN
        SELECT [status] = @StatusNaoOk, mensagem = N'Investidor e ProdutoComprado devem ser informados.';
        RETURN;
    END;

    IF @InvestidorTrim = '' OR @ProdutoTrim = ''
    BEGIN
        SELECT [status] = @StatusNaoOk, mensagem = N'Investidor e ProdutoComprado nao podem ser vazios.';
        RETURN;
    END;

    IF @MaxProfundidade < 1
    BEGIN
        SELECT [status] = @StatusNaoOk, mensagem = N'MaxProfundidade deve ser maior ou igual a 1.';
        RETURN;
    END;

    IF @InvestidorTrim = @ProdutoTrim
    BEGIN
        SELECT
            [status] = @StatusNaoOk,
            mensagem = CONCAT(N'Conflito: auto-relacionamento ', @InvestidorTrim, N' -> ', @ProdutoTrim, N' gera ciclo imediato.');
        RETURN;
    END;

    -- Eu trato duplicado como conflito para manter o grafo sem arestas redundantes.
    IF EXISTS
    (
        SELECT 1
        FROM dbo.Dependencias d
        WHERE d.Investidor = @InvestidorTrim
          AND d.ProdutoComprado = @ProdutoTrim
    )
    BEGIN
        SELECT
            [status] = @StatusNaoOk,
            mensagem = CONCAT(N'Conflito: relacionamento ', @InvestidorTrim, N' -> ', @ProdutoTrim, N' ja existe.');
        RETURN;
    END;

    ;WITH Caminhos AS
    (
        -- Eu começo a busca no ProdutoComprado porque a regra pede provar B -> ... -> A.
        SELECT
            Atual = d.ProdutoComprado,
            PathNodes = CAST(CONCAT('|', @ProdutoTrim, '|', d.ProdutoComprado, '|') AS VARCHAR(4000)),
            PathTexto = CAST(CONCAT(@ProdutoTrim, ' -> ', d.ProdutoComprado) AS VARCHAR(4000)),
            Profundidade = 1
        FROM dbo.Dependencias d
        WHERE d.Investidor = @ProdutoTrim

        UNION ALL

        SELECT
            Atual = d.ProdutoComprado,
            PathNodes = CAST(CONCAT(c.PathNodes, d.ProdutoComprado, '|') AS VARCHAR(4000)),
            PathTexto = CAST(CONCAT(c.PathTexto, ' -> ', d.ProdutoComprado) AS VARCHAR(4000)),
            Profundidade = c.Profundidade + 1
        FROM Caminhos c
        INNER JOIN dbo.Dependencias d
            ON d.Investidor = c.Atual
        WHERE c.Profundidade < @MaxProfundidade
          -- Eu guardo nodes visitados para impedir recursão cíclica dentro da própria consulta.
          AND CHARINDEX(CONCAT('|', d.ProdutoComprado, '|'), c.PathNodes) = 0
    )
    SELECT TOP (1)
        @Caminho = PathTexto
    FROM Caminhos
    WHERE Atual = @InvestidorTrim
    ORDER BY Profundidade ASC
    -- Eu deixo MAXRECURSION alto e controlo o limite real pelo @MaxProfundidade.
    OPTION (MAXRECURSION 32767);

    IF @Caminho IS NULL
    BEGIN
        SELECT [status] = N'Ok', mensagem = N'Sem conflito';
        RETURN;
    END;

    SET @Mensagem = CONCAT(
        N'Conflito detectado: ja existe caminho ',
        @Caminho,
        N'. Inserir ',
        @InvestidorTrim, N' -> ', @ProdutoTrim,
        -- Eu monto o ciclo completo para facilitar diagnóstico operacional.
        N' fecha ciclo: ',
        @InvestidorTrim, N' -> ', @Caminho, N' -> ', @InvestidorTrim, N'.'
    );

    SELECT [status] = @StatusNaoOk, mensagem = @Mensagem;
END;
GO
