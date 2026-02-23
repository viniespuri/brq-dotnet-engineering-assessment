/*
Case 5
Cria estrutura da tabela de dependencias.
*/

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.Dependencias', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Dependencias;
END;
GO

CREATE TABLE dbo.Dependencias
(
    Investidor      VARCHAR(100) NOT NULL,
    ProdutoComprado VARCHAR(100) NOT NULL,
    -- Eu valido campo em branco no banco para não depender só da aplicação.
    CONSTRAINT CK_Dependencias_Investidor_NotBlank CHECK (LTRIM(RTRIM(Investidor)) <> ''),
    -- Eu aplico a mesma proteção para o destino da relação.
    CONSTRAINT CK_Dependencias_Produto_NotBlank CHECK (LTRIM(RTRIM(ProdutoComprado)) <> ''),
    -- Eu bloqueio auto-loop já na origem do dado para reduzir lixo lógico.
    CONSTRAINT CK_Dependencias_NoSelfLoop CHECK (Investidor <> ProdutoComprado)
);
GO

-- Eu assumi duplicidade como conflito de negócio; este índice garante isso estruturalmente.
CREATE UNIQUE INDEX UX_Dependencias_Investidor_Produto
    ON dbo.Dependencias (Investidor, ProdutoComprado);
GO

-- Eu indexo os dois lados porque a validação percorre o grafo por origem e também consulta destino.
CREATE INDEX IX_Dependencias_Investidor
    ON dbo.Dependencias (Investidor);
GO

CREATE INDEX IX_Dependencias_ProdutoComprado
    ON dbo.Dependencias (ProdutoComprado);
GO
