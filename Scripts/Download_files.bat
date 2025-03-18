@echo off
REM Script para baixar arquivos do S3 e converter para JSON

REM Defina o bucket e a pasta de destino
set BUCKET=s3://bucket1.sa-east-1.acg.certificadoranacional.com
set DESTINATION=C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Inventarios

REM Criar a pasta de destino se não existir
if not exist "%DESTINATION%" (
    mkdir "%DESTINATION%"
)

REM Ler IDs do arquivo de texto e baixar arquivos
for /F "delims=" %%i in (C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Scripts\ids_unique.txt) do (
    REM Baixar o arquivo
    aws s3 cp %BUCKET%/%%i %DESTINATION%\

    REM Converter o arquivo para JSON
    set INPUT_FILE=%DESTINATION%\%%i
    set OUTPUT_FILE=%DESTINATION%\%%i.json

    (
        echo {
        echo     "content": "
        type "%INPUT_FILE%"
        echo     "
        echo }
    ) > "%OUTPUT_FILE%"
)

echo Download e conversão concluídos!
pause