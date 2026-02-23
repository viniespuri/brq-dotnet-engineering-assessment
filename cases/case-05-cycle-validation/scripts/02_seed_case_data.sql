/*
Case 5 - Script 02
Carga de dados do enunciado.
*/

SET NOCOUNT ON;
GO

INSERT INTO dbo.Dependencias (Investidor, ProdutoComprado)
VALUES
    ('Produto 1', 'Produto 2'),
    ('Produto 1', 'Produto 3'),
    ('Produto 2', 'Produto 4'),
    ('Produto 3', 'Produto 5');
GO
