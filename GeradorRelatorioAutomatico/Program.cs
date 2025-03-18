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

class Program
{
    static void Main()
    {
        string excelPath = @"C:\Users\gustavo.fernandes\Desktop\inventario.xlsx";
        string jsonFolderPath = @"C:\Users\gustavo.fernandes\Desktop\coletarinventariosS3\Inventarios";
        string outputPdf = @"C:\Users\gustavo.fernandes\Desktop\Inventario_Ativos.pdf";
        
        GlobalFontSettings.FontResolver = new CustomFontResolver();
        // Removed or commented out as CustomFontResolver is not defined.
        
        // Carregar dados do Excel
        var equipamentos = LerDadosExcel(excelPath);
        
        // Processar arquivos JSON
        ProcessarArquivosJson(jsonFolderPath, equipamentos);
        
        // Gerar PDF
        GerarPdf(outputPdf, equipamentos);
    }

    static List<Equipamento> LerDadosExcel(string excelPath)
    {
        List<Equipamento> equipamentos = new();
        FileInfo fileInfo = new FileInfo(excelPath);
        
        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            int rows = worksheet.Dimension.Rows;
            
            for (int row = 2; row <= rows; row++)
            {
                equipamentos.Add(new Equipamento
                {
                    Identificador = worksheet.Cells[row, 1].Text,
                    Marca = worksheet.Cells[row, 2].Text,
                    Modelo = worksheet.Cells[row, 3].Text
                });
            }
        }
        
        return equipamentos;
    }

    static void ProcessarArquivosJson(string jsonFolderPath, List<Equipamento> equipamentos)
    {
        foreach (var equipamento in equipamentos)
        {
            string jsonFile = Path.Combine(jsonFolderPath, equipamento.Identificador + ".json");
            if (File.Exists(jsonFile))
            {
                string jsonContent = File.ReadAllText(jsonFile);
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);
                
                // Extrair softwares instalados
                foreach (var item in data)
                {
                    if (item.ContainsKey("Programas_instalados_com_suas_versoes"))
                    {
                        equipamento.ProgramasInstalados = item["Programas_instalados_com_suas_versoes"].ToString();
                    }
                }
            }
        }
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
    public string? Identificador { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? ProgramasInstalados { get; set; }
}