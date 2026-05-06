using System.Text.Json;
using ClosedXML.Excel;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

public abstract class RelatorioConsolidadoTemplateBase : RelatorioTemplateBase
{
    private static readonly JsonSerializerOptions _debugJsonOptions = new() { WriteIndented = true };

    protected RelatorioConsolidadoTemplateBase(IServicoArmazenamentoMinio servicoArmazenamentoMinio)
        : base(servicoArmazenamentoMinio) { }

    protected static void EscreverCabecalhoConsolidado(IXLWorksheet sheet, RelatorioConsolidadoSondagemDto dto)
    {
        var dtoJson = JsonSerializer.Serialize(dto, _debugJsonOptions);
        _ = dtoJson;

        sheet.Row(1).Height = 50;
        sheet.Range(1, 1, 1, 6).Merge();
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.White;

        using var logoStream = ObterLogoPrefeitura();
        sheet.AddPicture(logoStream)
            .MoveTo(sheet.Cell(1, 1))
            .WithSize(220, 45);

        sheet.Row(2).Height = 25;
        sheet.Range(2, 1, 2, 6).Merge();
        var tituloCell = sheet.Cell(2, 1);
        tituloCell.Value = "Relatório da Sondagem - Escrita consolidado";
        tituloCell.Style.Font.Bold = true;
        tituloCell.Style.Font.FontColor = XLColor.White;
        tituloCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#490CF5");
        tituloCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        tituloCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Row(3).Height = 18;
        sheet.Row(4).Height = 18;
        sheet.Row(5).Height = 18;
        sheet.Row(6).Height = 18;

        EscreverCelulaRichText(sheet, 3, 1, 3, 2, "Agrupamento: ", dto.Agrupamento);
        EscreverCelulaRichText(sheet, 3, 3, 3, 4, "Ano letivo: ", dto.AnoLetivo > 0 ? dto.AnoLetivo.ToString() : "-");
        EscreverCelulaRichText(sheet, 3, 5, 3, 6, "Etapa / Modalidade: ", dto.Modalidade);

        EscreverCelulaRichText(sheet, 4, 1, 4, 2, "DRE: ", dto.Dre);
        EscreverCelulaRichText(sheet, 4, 3, 4, 4, "Unidade Educacional: ", dto.UnidadeEducacional);
        EscreverCelulaRichText(sheet, 4, 5, 4, 6, "Ano / Turma: ", dto.AnoTurma);

        EscreverCelulaRichText(sheet, 5, 1, 5, 2, "Componente Curricular: ", dto.ComponenteCurricular);
        EscreverCelulaRichText(sheet, 5, 3, 5, 4, "Proficiência: ", dto.Proficiencia);
        EscreverCelulaRichText(sheet, 5, 5, 5, 6, "Bimestre: ", dto.Bimestre);

        EscreverCelulaRichText(sheet, 6, 1, 6, 3, "Usuário: ", dto.Usuario);
        EscreverCelulaRichText(sheet, 6, 4, 6, 6, "Data de impressão: ", dto.DataImpressao.ToString("dd/MM/yyyy"));

        AplicarBordaCabecalho(sheet, 2, 1, 2, 6);
        AplicarBordaCabecalho(sheet, 3, 1, 3, 2);
        AplicarBordaCabecalho(sheet, 3, 3, 3, 4);
        AplicarBordaCabecalho(sheet, 3, 5, 3, 6);
        AplicarBordaCabecalho(sheet, 4, 1, 4, 2);
        AplicarBordaCabecalho(sheet, 4, 3, 4, 4);
        AplicarBordaCabecalho(sheet, 4, 5, 4, 6);
        AplicarBordaCabecalho(sheet, 5, 1, 5, 2);
        AplicarBordaCabecalho(sheet, 5, 3, 5, 4);
        AplicarBordaCabecalho(sheet, 5, 5, 5, 6);
        AplicarBordaCabecalho(sheet, 6, 1, 6, 3);
        AplicarBordaCabecalho(sheet, 6, 4, 6, 6);
    }

    private static void EscreverCelulaRichText(IXLWorksheet sheet, int row1, int col1, int row2, int col2, string label, string? value)
    {
        if (row1 != row2 || col1 != col2)
            sheet.Range(row1, col1, row2, col2).Merge();

        var cell = sheet.Cell(row1, col1);
        var effectiveValue = string.IsNullOrWhiteSpace(value) ? "-" : value;

        cell.Value = $"{label}{effectiveValue}";
        cell.Style.Font.Bold = true;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
    }

    private static void AplicarBordaCabecalho(IXLWorksheet sheet, int row1, int col1, int row2, int col2)
    {
        var range = sheet.Range(row1, col1, row2, col2);
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.OutsideBorderColor = XLColor.Black;
    }

    protected static Stream ObterLogoPrefeitura()
    {
        var assembly = typeof(RelatorioConsolidadoTemplateBase).Assembly;
        const string resourceName = "SME.Sondagem.MS.Relatorios.Excel.Resources.prefeitura.png";
        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException("Embedded resource prefeitura.png not found");
    }
}
