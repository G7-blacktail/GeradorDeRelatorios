using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

public class CustomFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts", faceName + ".ttf");

        if (!File.Exists(fontPath))
        {
            Console.WriteLine($"⚠️ Fonte não encontrada: {fontPath}");
            return null;  // Retorna null ao invés de lançar uma exceção
        }

        return File.ReadAllBytes(fontPath);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string fontFile = "Arial-Regular"; // Nome base

        if (isBold) fontFile = "Arial-Bold";
        if (isItalic) fontFile = "Arial-Italic";

        return new FontResolverInfo(fontFile);
    }
}

