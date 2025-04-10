-- Primeira opção de consulta

-- WITH Inventarios AS (
--     SELECT 
--         tu.nm_usuario,
--         tuc.nr_documento,
--         tia.id_arquivo_json,
--         ta.id_binario,
--         tia.dt_inventario,
--         ROW_NUMBER() OVER (PARTITION BY tuc.nr_documento ORDER BY tia.dt_inventario DESC) AS rn
--     FROM tb_usuario_cadastro tuc
--     INNER JOIN tb_usuario tu ON tuc.id_usuario = tu.id_usuario
--     INNER JOIN tb_inventario_automatico tia ON tia.nr_cpf = tuc.nr_documento
--     INNER JOIN tb_arquivo ta ON ta.id_arquivo = tia.id_arquivo_json
--     WHERE tuc.tp_situacao IN (1, 5, 90, 91, 92, 93)
-- )
-- SELECT 
--     nm_usuario,
--     nr_documento,
--     id_arquivo_json,
--     id_binario,
--     dt_inventario
-- FROM Inventarios
-- WHERE rn = 1

-- UNION ALL

-- SELECT 
--     tu.nm_usuario,
--     tuc.nr_documento,
--     tia.id_arquivo_json,
--     ta.id_binario,
--     tia.dt_inventario
-- FROM tb_usuario_cadastro tuc
-- INNER JOIN tb_usuario tu ON tuc.id_usuario = tu.id_usuario
-- INNER JOIN tb_inventario_automatico tia ON tia.nr_cpf = tuc.nr_documento
-- INNER JOIN tb_arquivo ta ON ta.id_arquivo = tia.id_arquivo_json
-- WHERE tuc.tp_situacao IN (1, 5, 90, 91, 92, 93)
-- AND NOT EXISTS (
--     SELECT 1
--     FROM tb_inventario_automatico tia2
--     WHERE tia2.nr_cpf = tuc.nr_documento
--     AND tia2.dt_inventario BETWEEN '2025-02-01 00:00:00.0000' AND '2025-02-28 23:59:59.00000'
-- )
-- AND tia.dt_inventario = (
--     SELECT MAX(tia3.dt_inventario)
--     FROM tb_inventario_automatico tia3
--     WHERE tia3.nr_cpf = tuc.nr_documento
-- )
-- ORDER BY 
--     nm_usuario;


--- Segunda opção de consulta:

 WITH Inventarios AS (
    SELECT 
    	tu.id_usuario,
        tu.nm_usuario,
        tuc.nr_documento,
        tia.id_arquivo_json,
        ta.id_binario,
        tia.dt_inventario,
        ROW_NUMBER() OVER (PARTITION BY tuc.nr_documento ORDER BY tia.dt_inventario DESC) AS rn
    FROM tb_usuario_cadastro tuc
    INNER JOIN tb_usuario tu ON tuc.id_usuario = tu.id_usuario
    INNER JOIN tb_inventario_automatico tia ON tia.nr_cpf = tuc.nr_documento
    INNER JOIN tb_arquivo ta ON ta.id_arquivo = tia.id_arquivo_json
    WHERE tuc.tp_situacao IN (1, 90, 91, 92, 93)
    AND tia.dt_inventario < '2025-04-01' -- :data_limite
)
SELECT 
    nm_usuario,
    nr_documento,
    id_arquivo_json,
    id_binario,
    dt_inventario
FROM Inventarios i
inner join tb_usuario_comodato tuco ON i.id_usuario = tuco.id_usuario
WHERE i.rn = 1 
ORDER BY i.nm_usuario;