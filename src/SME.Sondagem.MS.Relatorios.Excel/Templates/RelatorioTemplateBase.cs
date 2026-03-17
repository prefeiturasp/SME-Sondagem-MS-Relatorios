using ClosedXML.Excel;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

public abstract class RelatorioTemplateBase
{
    protected static int EscreverTitulo(IXLWorksheet sheet, string tituloRelatorio, string? subTituloRelatorio, int linha)
    {
        var titulo = sheet.Cell(linha, 1);
        titulo.Value = tituloRelatorio;
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Range(linha, 1, linha, 10).Merge();
        linha++;

        var subtitulo = sheet.Cell(linha, 1);
        subtitulo.Value = subTituloRelatorio;
        subtitulo.Style.Font.Bold = true;
        subtitulo.Style.Font.FontSize = 12;
        subtitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Range(linha, 1, linha, 10).Merge();
        linha += 2;

        return linha;
    }

    protected static void EstilarEPreencher(IXLCell celula, object valor)
    {
        celula.Value = valor?.ToString();
        EstilarCelulaDados(celula);
    }

    protected static XLColor ConverterCor(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            return XLColor.White;

        var hex = cor.TrimStart('#');

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return XLColor.FromArgb(r, g, b);
    }

    protected static void PreencherCelulaSondagem(IXLCell cell, string valor, XLColor corFundo)
    {
        bool vazio = string.IsNullOrWhiteSpace(valor)
                     || valor == "Sem preencher"
                     || valor == "Vazio";

        cell.Value = valor;

        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Font.Bold = true;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        cell.Style.Fill.BackgroundColor = vazio ? XLColor.White : corFundo;
        cell.Style.Font.FontColor = vazio ? XLColor.Gray : XLColor.White;
    }


    protected static void EstilizarHeader(IXLCell cell, string texto)
    {
        cell.Value = texto;
        cell.Style.Font.Bold = true;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    protected static void EscreverCelula(IXLWorksheet ws, int row, int col, string valor, bool bold = false)
    {
        var cell = ws.Cell(row, col);
        cell.Value = valor;
        cell.Style.Font.Bold = bold;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
    }

    protected static void AplicarBordaExterna(IXLRange range)
    {
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    protected static void EstilarCelulaDados(IXLCell cell)
    {
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    protected static Stream ObterLogo()
    {
        string base64Logo = SmeConstants.LogoSmeMono.Substring(SmeConstants.LogoSmeMono.IndexOf(',') + 1);
        return new MemoryStream(Convert.FromBase64String(base64Logo));
    }

    protected static void ConfigurarCabecalho(IXLWorksheet sheet)
    {
        sheet.Row(1).Height = 20;
        sheet.Row(2).Height = 20;
        sheet.Row(3).Height = 20;
    }

}
