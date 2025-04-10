using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;
using Newtonsoft.Json;
using OfficeOpenXml;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using System.Linq;
using System.Globalization;


class Program
{
    static void Main()
    {
        string csvPath = @"C:\Users\gustavo.fernandes\Desktop\inventario.csv";
        string jsonFolderPath = @"C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Inventarios";
        string outputPdf = @"C:\Users\gustavo.fernandes\Desktop\Inventario_Ativos.pdf";

         GlobalFontSettings.FontResolver = new CustomFontResolver();
        
        // Ler e processar arquivos JSON
        List<Comodato> equipamentos = ProcessarJsons(jsonFolderPath, csvPath);
        
        // Gerar PDF
        GerarPdf(outputPdf, equipamentos);
    }

    static List<Comodato> ProcessarJsons(string jsonFolderPath, string csvPath)
    {
        List<Comodato> equipamentos = new();
        var csvData = LerCsv(csvPath);
        
        foreach (string file in Directory.GetFiles(jsonFolderPath, "*.json"))
        { 
            string id_binario = Path.GetFileName(file);
            var equipamento = csvData.FirstOrDefault(e => e.IdBinario == id_binario);
            
            if (equipamento != null)
            {
                // Coletar os DisplayNames do JSON
                equipamento.ProgramasInstalados = string.Join(Environment.NewLine, ObterDisplayNames(file));
                equipamentos.Add(equipamento);
            }
        }
        return equipamentos;
    }

    static List<Comodato> LerCsv(string csvPath)
    {
        var linhas = File.ReadAllLines(csvPath);
        var equipamentos = new List<Comodato>();
        var idsBinarios = new HashSet<string>(); // Para rastrear IDs únicos

        // Verifica se há pelo menos duas linhas (cabeçalho + dados)
        if (linhas.Length < 2)
        {
            return equipamentos; // Retorna lista vazia se não houver dados suficientes
        }

        // Lê a partir da segunda linha (índice 1)
        for (int i = 1; i < linhas.Length; i++)
        {
            var colunas = ParseCsvLine(linhas[i]);

            // Verifica se há colunas suficientes
            if (colunas.Length >= 15)
            {
                var idBinario = colunas[3] + ".json"; // id_binario

                // Verifica se o ID binário já foi adicionado
                if (!idsBinarios.Contains(idBinario))
                {
                    idsBinarios.Add(idBinario); // Adiciona o ID binário ao conjunto

                    equipamentos.Add(new Comodato
                    {
                        NomeUsuario = colunas[0],  // nm_usuario
                        Email = colunas[1],         // ds_email
                        Documento = colunas[2],     // nr_documento
                        IdBinario = idBinario,       // id_binario
                        DataCriacaoAud = colunas[4],// dt_criacao_aud
                        Identificador = colunas[5],  // cd_comodato
                        Marca = colunas[6],         // ds_marca_equipamento
                        Modelo = colunas[7],        // ds_modelo_equipamento
                        DataInventario = colunas[8],// dt_inventario
                        EspecificacaoEquipamento = colunas[9], // ds_especificacao_equipamento
                        MarcaCamera = colunas.Length > 10 ? colunas[10] : "",  // ds_marca_camera
                        ModeloCamera = colunas.Length > 11 ? colunas[11] : "",  // ds_modelo_camera
                        EspecificacaoCamera = colunas.Length > 12 ? colunas[12] : "", // ds_especificacao_camera
                        MarcaLeitor = colunas.Length > 13 ? colunas[13] : "",   // ds_marca_leitor
                        ModeloLeitor = colunas.Length > 14 ? colunas[14] : "",   // ds_modelo_leitor
                        EspecificacaoLeitor = colunas.Length > 15 ? colunas[15] : "" // ds_especificacao_leitor
                    });
                }
            }
        }

        return equipamentos;
    }
// Método para tratar linhas CSV corretamente
    static string[] ParseCsvLine(string linha)
    {
        List<string> campos = new List<string>();
        bool dentroDeAspas = false;
        string campoAtual = "";

        for (int i = 0; i < linha.Length; i++)
        {
            char c = linha[i];

            if (c == '"')
            {
                dentroDeAspas = !dentroDeAspas; // Alterna dentro/fora de aspas
            }
            else if (c == ';' && !dentroDeAspas) // Mudamos para ';' como delimitador
            {
                campos.Add(campoAtual.Trim().Replace("\"", "") == "" ? "null" : campoAtual.Trim().Replace("\"", "")); // Adiciona o campo e limpa
                campoAtual = "";
            }
            else
            {
                campoAtual += c; // Continua adicionando ao campo
            }
        }

        // Adiciona o último campo, mesmo que esteja vazio
        campos.Add(campoAtual.Trim().Replace("\"", "") == "" ? "null" : campoAtual.Trim().Replace("\"", ""));

        return campos.ToArray();
    }

    static string[] ObterDisplayNames(string jsonFilePath)
    {
        // Lê o conteúdo do arquivo JSON
        string jsonContent = File.ReadAllText(jsonFilePath);
        
        // Deserializa o JSON em uma lista de objetos
        var dados = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);
        
        var displayNames = new List<string>();

        // Itera sobre cada objeto no array
        foreach (var item in dados!)
        {
            // Verifica se o item contém a chave "Programas_instalados_com_suas_versoes"
            if (item.ContainsKey("Programas_instalados_com_suas_versoes"))
            {
                // Obtém a lista de softwares
                var softwares = item["Programas_instalados_com_suas_versoes"] as Newtonsoft.Json.Linq.JArray;

                // Itera sobre cada software e coleta os DisplayNames
                if (softwares != null)
                {
                    foreach (var software in softwares)
                    {
                        var displayName = software["DisplayName"]?.ToString();
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            displayNames.Add(displayName);
                        }
                    }
                }
            }
        }

        return displayNames.ToArray(); // Retorna um array de strings
    }

    [Obsolete("This method is marked as obsolete. Consider refactoring or removing it in the future.")]
    static void GerarPdf(string outputPdf, List<Comodato> equipamentos)
    {
        using (PdfDocument document = new PdfDocument())
        {
            foreach (var equipamento in equipamentos)
            {
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new("Arial", 12, XFontStyleEx.Regular);

                // Posição inicial
                double posY = 50;
                double marginLeft = 50;
                double pageWidth = page.Width - 100; // Margens esquerda e direita
                double pageHeight = page.Height - 100; // Margem superior e inferior

                // Função para desenhar texto com quebra de linha
                void DrawStringWithWrap(string text, double x, ref double y, double maxWidth, PdfPage currentPage)
                {
                    string[] words = text.Split(' ');
                    string line = "";

                    foreach (var word in words)
                    {
                        string testLine = line + word + " ";
                        double lineWidth = gfx.MeasureString(testLine, font).Width;

                            if (lineWidth > maxWidth)
                            {
                                // Verifica se a nova linha ultrapassa a altura da página
                                if (y + 20 > pageHeight)
                                {
                                    // Adiciona uma nova página
                                    currentPage = document.AddPage();
                                    gfx = XGraphics.FromPdfPage(currentPage);
                                    y = 50; // Reinicia a posição Y
                                }

                                // Desenha a linha atual e reseta
                                gfx.DrawString(line, font, XBrushes.Black, new XPoint(x, y));
                                y += 20; // Incrementa a posição Y para a próxima linha
                                line = word + " "; // Começa uma nova linha
                            }
                            else
                            {
                                line = testLine; // Continua a linha
                            }
                        }

                        // Desenha a última linha
                        if (!string.IsNullOrEmpty(line))
                        {
                            // Verifica se a nova linha ultrapassa a altura da página
                            if (y + 20 > pageHeight)
                            {
                                // Adiciona uma nova página
                                currentPage = document.AddPage();
                                gfx = XGraphics.FromPdfPage(currentPage);
                                y = 50; // Reinicia a posição Y
                            }

                            gfx.DrawString(line, font, XBrushes.Black, new XPoint(x, y));
                            y += 20; // Incrementa a posição Y para a próxima linha
                        }
                    }

                    // Desenhando as informações do equipamento
                    DrawStringWithWrap("Identificação", marginLeft, ref posY, pageWidth, page);
                    posY += 10; // Espaço extra entre seções

                    DrawStringWithWrap($"Equipamento: {equipamento.Marca} {equipamento.Modelo}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Identificador: {equipamento.Identificador}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Marca: {equipamento.Marca}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Modelo: {equipamento.Modelo}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Data de Criação: {equipamento.DataCriacaoAud}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Usuários: <Nada aqui ainda>", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Especificações do Equipamento: {equipamento.EspecificacaoEquipamento}", marginLeft, ref posY, pageWidth, page);
                    posY += 20; // Espaço extra antes da lista de programas instalados

                    DrawStringWithWrap("Programas Instalados:", marginLeft, ref posY, pageWidth, page);
                    posY += 10; // Espaço extra antes da lista de softwares

                    var softwaresInstalados = equipamento.ProgramasInstalados.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    foreach (var software in softwaresInstalados)
                    {
                        DrawStringWithWrap(software, marginLeft, ref posY, pageWidth, page);
                    }
                    posY += 20; // Espaço extra antes da próxima seção
                    DrawStringWithWrap($"Marca da camera: {equipamento.MarcaCamera}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Modelo da camera: {equipamento.ModeloCamera}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Especificações da Camera: {equipamento.EspecificacaoCamera}", marginLeft, ref posY, pageWidth, page);

                    posY += 20; // Espaço extra antes da próxima seção
                    DrawStringWithWrap($"Marca do Leitor: {equipamento.MarcaLeitor}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Modelo do Leitor: {equipamento.ModeloLeitor}", marginLeft, ref posY, pageWidth, page);
                    DrawStringWithWrap($"Especificações do Leitor: {equipamento.EspecificacaoLeitor}", marginLeft, ref posY, pageWidth, page);


                }
                document.Save(outputPdf);
        }
    }
}

class Comodato
{
    public string ProgramasInstalados { get; set; } = "";
    public string NomeUsuario { get; set; } = ""; // nm_usuario
    public string Email { get; set; } = ""; // ds_email
    public string Documento { get; set; } = ""; // nr_documento
    public string IdBinario { get; set; } = ""; // id_binario
    public string DataCriacaoAud { get; set; } = ""; // dt_criacao_aud
    public string Identificador { get; set; } = ""; // cd_comodato
    public string Marca { get; set; } = ""; // ds_marca_equipamento
    public string Modelo { get; set; } = ""; // ds_modelo_equipamento
    public string DataInventario { get; set; } = ""; // dt_inventario
    public string EspecificacaoEquipamento { get; set; } = ""; // ds_especificacao_equipamento
    public string MarcaCamera { get; set; } = ""; // ds_marca_camera
    public string ModeloCamera { get; set; } = ""; // ds_modelo_camera
    public string EspecificacaoCamera { get; set; } = ""; // ds_especificacao_camera
    public string MarcaLeitor { get; set; } = ""; // ds_marca_leitor
    public string ModeloLeitor { get; set; } = ""; // ds_modelo_leitor
    public string EspecificacaoLeitor { get; set; } = ""; // ds_especificacao_leitor

}

// Classe que representa a estrutura do JSON
public class RootObject
{
    public List<Software> Programas_instalados_com_suas_versoes { get; set; }
}

public class Software
{
    public string DisplayName { get; set; }
}