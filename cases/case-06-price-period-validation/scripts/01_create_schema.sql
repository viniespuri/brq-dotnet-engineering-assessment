/*
Case 6
Cria estrutura da tabela de precos.
*/

SET NOCOUNT ON;
GO

IF OBJECT_ID('dbo.Precos', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Precos;
END;
GO

CREATE TABLE dbo.Precos
(
    Produto        VARCHAR(100) NOT NULL,
    DataReferencia DATETIME2(0) NOT NULL,
    Preco          DECIMAL(18, 4) NOT NULL,
    CONSTRAINT CK_Precos_Produto_NotBlank CHECK (LTRIM(RTRIM(Produto)) <> ''),
    CONSTRAINT CK_Precos_Preco_NaoNegativo CHECK (Preco >= 0)
);
GO

CREATE INDEX IX_Precos_Produto_DataReferencia
    ON dbo.Precos (Produto, DataReferencia DESC);
GO
