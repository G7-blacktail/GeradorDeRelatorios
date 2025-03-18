# Defina o diretório
$directory = "C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Inventarios"

# Mude para o diretório especificado
Set-Location -Path $directory

# Loop por todos os arquivos no diretório
Get-ChildItem -File | ForEach-Object {
    # Defina o novo nome com a extensão .json
    $newName = "$($_.BaseName).json"

    # Renomear o arquivo para ter a extensão .json
    Rename-Item -Path $_.FullName -NewName $newName -Force
}

Write-Host "Extensões alteradas para .json!"