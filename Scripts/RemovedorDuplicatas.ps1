# Defina o caminho do arquivo de entrada e saída
$inputFile = "C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Scripts\ids.txt"
$outputFile = "C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Scripts\ids_unique.txt"

# Ler o arquivo, remover duplicatas e salvar em um novo arquivo
Get-Content -Path $inputFile | Sort-Object -Unique | Set-Content -Path $outputFile

Write-Host "Duplicatas removidas e resultado salvo em '$outputFile'."