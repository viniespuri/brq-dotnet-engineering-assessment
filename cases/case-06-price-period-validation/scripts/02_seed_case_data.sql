/*
Case 6 - Script 02
Insere dados de exemplo para validar regras de janela por horario.
*/

SET NOCOUNT ON;
GO

INSERT INTO dbo.Precos (Produto, DataReferencia, Preco)
VALUES
    ('PROD-A', '2026-02-22 08:00:00', 100.00), -- ontem
    ('PROD-A', '2026-02-20 08:00:00', 98.00),  -- 3 dias anteriores
    ('PROD-A', '2026-02-10 08:00:00', 95.00),  -- 13 dias anteriores
    ('PROD-A', '2026-01-25 08:00:00', 90.00),  -- 29 dias anteriores
    ('PROD-A', '2026-01-10 08:00:00', 80.00),  -- fora de 30 dias
    ('PROD-B', '2026-02-20 09:00:00', 70.00),  -- dentro de 5/30 dias
    ('PROD-B', '2026-02-10 09:00:00', 60.00);  -- dentro de 30 dias
GO
