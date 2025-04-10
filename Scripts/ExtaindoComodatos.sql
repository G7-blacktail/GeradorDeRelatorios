SELECT 
	 tu.nm_usuario,
	 tu.ds_email,
	 tuca.nr_documento,
	 tav.id_binario,
	 tuc.dt_criacao_aud,
	 tuc.cd_comodato,
	 tuc.ds_marca_equipamento,
	 tuc.ds_modelo_equipamento,
	 tia.dt_inventario,
	 tuc.ds_especificacao_equipamento,
	 tuc.ds_marca_camera,
	 tuc.ds_modelo_camera,
	 tuc.ds_especificacao_camera,
	 tuc.ds_marca_leitor,
	 tuc.ds_modelo_leitor,
	 tuc.ds_especificacao_leitor
FROM 
    tb_usuario_comodato tuc 
INNER JOIN 
    tb_usuario tu ON tuc.id_usuario = tu.id_usuario
Inner Join tb_usuario_cadastro tuca ON tu.id_usuario = tuca.id_usuario
INNER JOIN 
	tb_inventario_automatico tia ON tu.ds_email = tia.ds_email 
INNER JOIN tb_arquivo tav ON tia.id_arquivo_json = tav.id_arquivo 
WHERE 
--   tia.dt_inventario BETWEEN '2025-02-01 00:00:00.00000' AND '2025-02-28 23:59:59.0000' and
   tuca.tp_situacao in (1, 90, 92, 93, 94)
   group BY 
   nm_usuario
  order by nm_usuario;