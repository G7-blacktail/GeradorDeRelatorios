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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
        List<Equipamento> equipamentos = ProcessarJsons(jsonFolderPath, csvPath);
        
        // Gerar PDF
        GerarPdf(outputPdf, equipamentos);
    }

    static List<Equipamento> ProcessarJsons(string jsonFolderPath, string csvPath)
    {
        List<Equipamento> equipamentos = new();
        var csvData = LerCsv(csvPath);
        
        foreach (string file in Directory.GetFiles(jsonFolderPath, "*.json"))
        {
            string id_binario = Path.GetFileNameWithoutExtension(file);
            var equipamento = csvData.FirstOrDefault(e => e.Identificador == id_binario);
            
            if (equipamento != null)
            {
                equipamento.ProgramasInstalados = ProcessarJson(file);
                equipamentos.Add(equipamento);
            }
        }
        return equipamentos;
    }

static List<Equipamento> LerCsv(string csvPath)
{
    List<Equipamento> equipamentos = new();
    var linhas = File.ReadAllLines(csvPath);

    foreach (var linha in linhas.Skip(1)) // Pular cabeçalho
    {
        var colunas = ParseCsvLine(linha);
        
        if (colunas.Length >= 3)  // Verifica se há colunas suficientes
        {
            equipamentos.Add(new Equipamento
            {
                Identificador = colunas[3],  // Ajuste para corresponder à posição correta
                Marca = colunas[6],
                Modelo = colunas[7]
            });
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

    foreach (char c in linha)
    {
        if (c == '"' && (campoAtual.Length == 0 || dentroDeAspas)) 
        {
            dentroDeAspas = !dentroDeAspas; // Alterna dentro/fora de aspas
        }
        else if (c == ',' && !dentroDeAspas)
        {
            campos.Add(campoAtual.Trim()); // Adiciona o campo e limpa
            campoAtual = "";
        }
        else
        {
            campoAtual += c; // Continua adicionando ao campo
        }
    }

    campos.Add(campoAtual.Trim()); // Adiciona o último campo
    return campos.ToArray();
}

    static string ProcessarJson(string jsonFile)
    {
        string jsonContent = File.ReadAllText(jsonFile);
        var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);
        
        var softwares = data?.FirstOrDefault(d => d.ContainsKey("Programas_instalados_com_suas_versoes"))?
                          ["Programas_instalados_com_suas_versoes"]?.ToString();
        
        return softwares ?? "Nenhum software listado";
    }

    static void GerarPdf(string outputPdf, List<Equipamento> equipamentos)
    {
        using (PdfDocument document = new PdfDocument())
        {
            foreach (var equipamento in equipamentos)
            {
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new("arial", 12 , XFontStyleEx.Regular);
                
                gfx.DrawString($"Equipamento: {equipamento.Marca} {equipamento.Modelo}", font, XBrushes.Black, new XPoint(50, 50));
                gfx.DrawString($"Programas Instalados: {equipamento.ProgramasInstalados}", font, XBrushes.Black, new XPoint(50, 80));
            }
            document.Save(outputPdf);
        }
    }
}

class Equipamento
{
    public string Identificador { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string ProgramasInstalados { get; set; } = "";
}