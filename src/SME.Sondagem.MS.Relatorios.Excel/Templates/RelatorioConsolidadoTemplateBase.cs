using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using ChartFormula = DocumentFormat.OpenXml.Drawing.Charts.Formula;
using Index = DocumentFormat.OpenXml.Drawing.Charts.Index;
using NumberingFormat = DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat;
using OrientationValues = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues;
using Values = DocumentFormat.OpenXml.Drawing.Charts.Values;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

public abstract class RelatorioConsolidadoTemplateBase : RelatorioTemplateBase
{
    private static readonly JsonSerializerOptions _debugJsonOptions = new() { WriteIndented = true };

    protected RelatorioConsolidadoTemplateBase(IServicoArmazenamentoMinio servicoArmazenamentoMinio)
        : base(servicoArmazenamentoMinio) { }

    protected static int EscreverCabecalhoConsolidado(IXLWorksheet sheet, RelatorioConsolidadoSondagemDto dto)
    {
        var dtoJson = JsonSerializer.Serialize(dto, _debugJsonOptions);
        _ = dtoJson;

        sheet.Row(1).Height = 70;
        sheet.Range(1, 1, 1, 6).Merge();
        sheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.White;

        using var logoStream = ObterLogoPrefeitura();
        sheet.AddPicture(logoStream)
            .MoveTo(sheet.Cell(1, 1))
            .WithSize(220, 72);

        sheet.Row(2).Height = 25;
        sheet.Range(2, 1, 2, 6).Merge();
        var tituloCell = sheet.Cell(2, 1);
        tituloCell.Value = "Relatório da Sondagem - Escrita consolidado";
        tituloCell.Style.Font.Bold = true;
        tituloCell.Style.Font.FontColor = XLColor.White;
        tituloCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#490CF5");
        tituloCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        tituloCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        static string t(string label, string? value) => label + (string.IsNullOrWhiteSpace(value) ? "-" : value);

        // Row 3: Agrupamento | AnoLetivo | Modalidade
        EscreverCelulaRichText(sheet, 3, 1, 3, 2, "Agrupamento: ", dto.Agrupamento);
        EscreverCelulaRichText(sheet, 3, 3, 3, 4, "Ano letivo: ", dto.AnoLetivo > 0 ? dto.AnoLetivo.ToString() : "-");
        EscreverCelulaRichText(sheet, 3, 5, 3, 6, "Etapa / Modalidade: ", dto.Modalidade);

        // Row 4: DRE | UE | Ano or Semestre (label/value depend on modalidade)
        var (anoLabel, anoValue) = ObterLabelValorAno(dto);
        EscreverCelulaRichText(sheet, 4, 1, 4, 2, "DRE: ", dto.Dre);
        EscreverCelulaRichText(sheet, 4, 3, 4, 4, "Unidade Educacional: ", dto.UnidadeEducacional);
        EscreverCelulaRichText(sheet, 4, 5, 4, 6, anoLabel, anoValue);

        // Row 5: ComponenteCurricular | Proficiência | Bimestre
        EscreverCelulaRichText(sheet, 5, 1, 5, 2, "Componente Curricular: ", dto.ComponenteCurricular);
        EscreverCelulaRichText(sheet, 5, 3, 5, 4, "Proficiência: ", dto.Proficiencia);
        EscreverCelulaRichText(sheet, 5, 5, 5, 6, "Bimestre: ", dto.Bimestre);

        // Row 6: Gênero | Raça | Língua Portuguesa como 2ª Língua
        EscreverCelulaRichText(sheet, 6, 1, 6, 2, "Gênero: ", dto.Genero);
        EscreverCelulaRichText(sheet, 6, 3, 6, 4, "Raça: ", dto.Raca);
        EscreverCelulaRichText(sheet, 6, 5, 6, 6, "L.P. como 2ª Língua: ", FormatarLinguaSegundaLingua(dto.PossuiLinguaPortuguesaSegundaLingua));

        int linhaAtual = 6;

        // Row 7 (conditional): Programas e Atendimentos
        bool temProgramas = dto.Pap == true || dto.Aee == true || dto.Deficiente == true;
        if (temProgramas)
        {
            linhaAtual++;
            var programas = new List<string>();
            if (dto.Pap == true) programas.Add("PAP");
            if (dto.Aee == true) programas.Add("AEE");
            if (dto.Deficiente == true) programas.Add("Deficiente");
            var programasValue = string.Join(", ", programas);
            EscreverCelulaRichText(sheet, linhaAtual, 1, linhaAtual, 6, "Programas e Atendimentos: ", programasValue);
            AjustarAlturaLinhaCabecalho(sheet, linhaAtual, 90.0, t("Programas e Atendimentos: ", programasValue));
            AplicarBordaCabecalho(sheet, linhaAtual, 1, linhaAtual, 6);
        }

        // Last row: Usuário | DataImpressao
        linhaAtual++;
        EscreverCelulaRichText(sheet, linhaAtual, 1, linhaAtual, 3, "Usuário: ", dto.Usuario);
        EscreverCelulaRichText(sheet, linhaAtual, 4, linhaAtual, 6, "Data de impressão: ", dto.DataImpressao.ToString("dd/MM/yyyy"));

        AjustarAlturaLinhaCabecalho(sheet, 3, 30.0,
            t("Agrupamento: ", dto.Agrupamento),
            t("Ano letivo: ", dto.AnoLetivo > 0 ? dto.AnoLetivo.ToString() : "-"),
            t("Etapa / Modalidade: ", dto.Modalidade));
        AjustarAlturaLinhaCabecalho(sheet, 4, 30.0,
            t("DRE: ", dto.Dre),
            t("Unidade Educacional: ", dto.UnidadeEducacional),
            t(anoLabel, anoValue));
        AjustarAlturaLinhaCabecalho(sheet, 5, 30.0,
            t("Componente Curricular: ", dto.ComponenteCurricular),
            t("Proficiência: ", dto.Proficiencia),
            t("Bimestre: ", dto.Bimestre));
        AjustarAlturaLinhaCabecalho(sheet, 6, 30.0,
            t("Gênero: ", dto.Genero),
            t("Raça: ", dto.Raca),
            t("L.P. como 2ª Língua: ", FormatarLinguaSegundaLingua(dto.PossuiLinguaPortuguesaSegundaLingua)));
        AjustarAlturaLinhaCabecalho(sheet, linhaAtual, 45.0,
            t("Usuário: ", dto.Usuario),
            t("Data de impressão: ", dto.DataImpressao.ToString("dd/MM/yyyy")));

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
        AplicarBordaCabecalho(sheet, 6, 1, 6, 2);
        AplicarBordaCabecalho(sheet, 6, 3, 6, 4);
        AplicarBordaCabecalho(sheet, 6, 5, 6, 6);
        AplicarBordaCabecalho(sheet, linhaAtual, 1, linhaAtual, 3);
        AplicarBordaCabecalho(sheet, linhaAtual, 4, linhaAtual, 6);

        return linhaAtual;
    }

    private static (string Label, string Value) ObterLabelValorAno(RelatorioConsolidadoSondagemDto dto) =>
        dto.ModalidadeId switch
        {
            (int)Modalidade.EJA => ("Semestre: ", FormatarSemestre(dto.SemestreId)),
            (int)Modalidade.Fundamental => ("Ano: ", string.IsNullOrWhiteSpace(dto.AnoTurma) ? "Todos" : dto.AnoTurma),
            _ => ("Ano / Turma: ", string.IsNullOrWhiteSpace(dto.AnoTurma) ? "Todos" : dto.AnoTurma)
        };

    private static string FormatarSemestre(int semestreId)
    {
        if (semestreId == 0) return "Todos";
        return Enum.TryParse(semestreId.ToString(), out Semestre s) ? s.ShortName() ?? "Todos" : "Todos";
    }

    private static string FormatarLinguaSegundaLingua(bool? valor) =>
        valor == true ? "Sim" : "Não";

    private static void AjustarAlturaLinhaCabecalho(IXLWorksheet sheet, int linha, double larguraColuna, params string[] textos)
    {
        const double alturaMinima = 18.0;
        var maxAltura = textos.Max(t => ComputarAltura(t, larguraColuna));
        sheet.Row(linha).Height = Math.Max(alturaMinima, maxAltura);
    }

    private static double ComputarAltura(string? texto, double larguraColuna)
    {
        const double alturaLinha = 15.0;
        const double charsPerUnit = 0.9;
        int charsPerLine = Math.Max(1, (int)(larguraColuna * charsPerUnit));
        int linhas = string.IsNullOrEmpty(texto) ? 1 : Math.Max(1, (int)Math.Ceiling((double)texto.Length / charsPerLine));
        return linhas * alturaLinha + 3;
    }

    protected static void AjustarAlturaLinha(IXLWorksheet sheet, int linha, string? texto, double larguraColuna = 30.0) =>
        sheet.Row(linha).Height = ComputarAltura(texto, larguraColuna);

    private static void EscreverCelulaRichText(IXLWorksheet sheet, int row1, int col1, int row2, int col2, string label, string? value)
    {
        if (row1 != row2 || col1 != col2)
            sheet.Range(row1, col1, row2, col2).Merge();

        var cell = sheet.Cell(row1, col1);
        var effectiveValue = string.IsNullOrWhiteSpace(value) ? "-" : value;

        var rt = cell.GetRichText();
        rt.AddText(label).Bold = true;
        rt.AddText(effectiveValue).Bold = false;

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
            ?? throw new FileNotFoundException("prefeitura.png não encontrado");
    }

    protected static void InjetarGraficosOpenXml(
        Stream stream,
        List<(string Titulo, List<GraficoDto> Dados, int ColStart, int ColCount, int RowGroup)> graficos,
        int linhaInicio,
        string sheetName,
        int dataRowBase,
        int dataColStart)
    {
        using var document = SpreadsheetDocument.Open(stream, true);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("WorkbookPart nulo.");
        var sheetInfo = workbookPart.Workbook!.Sheets!.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetInfo.Id!.Value!);

        var drawingsPart = worksheetPart.DrawingsPart ?? worksheetPart.AddNewPart<DrawingsPart>();

        // Preserve existing drawing (e.g. picture anchors added by ClosedXML)
        var wsDr = drawingsPart.WorksheetDrawing;
        if (wsDr == null)
        {
            wsDr = new Xdr.WorksheetDrawing();
            wsDr.AddNamespaceDeclaration("xdr", ExcelConstantes.OPENXML_FORMATS_DRAWING);
            wsDr.AddNamespaceDeclaration("a", ExcelConstantes.OPENXML_FORMATS_MAIN);
            wsDr.AddNamespaceDeclaration("r", ExcelConstantes.OPENXML_FORMATS_RELATIONSHIP);
            drawingsPart.WorksheetDrawing = wsDr;
        }

        for (int i = 0; i < graficos.Count; i++)
        {
            var (_, dados, colStart, colCount, rowGroup) = graficos[i];
            int dataRow = dataRowBase + i * 12;
            int dataEndCol = dataColStart + dados.Count - 1;
            string catFormula = $"'{sheetName}'!${ColNumToLetter(dataColStart)}${dataRow}:${ColNumToLetter(dataEndCol)}${dataRow}";
            string valFormula = $"'{sheetName}'!${ColNumToLetter(dataColStart)}${dataRow + 1}:${ColNumToLetter(dataEndCol)}${dataRow + 1}";

            var chartPart = drawingsPart.AddNewPart<ChartPart>();
            ConfigurarChartPartGrafico(chartPart, dados, catFormula, valFormula);

            var chartRelId = drawingsPart.GetIdOfPart(chartPart);
            int linhaGrafico = linhaInicio + rowGroup * 26 + 4;
            int fromCol = colStart - 1;
            int toCol = colStart - 1 + colCount; // exclusive right-edge anchor
            wsDr.AppendChild(CriarAnchorGrafico(chartRelId, linhaGrafico, (uint)(10 + i), fromCol, toCol));
        }

        wsDr.Save();

        if (!worksheetPart.Worksheet.Elements<Drawing>().Any())
        {
            var drawingRelId = worksheetPart.GetIdOfPart(drawingsPart);
            worksheetPart.Worksheet.AppendChild(new Drawing { Id = drawingRelId });
            worksheetPart.Worksheet.Save();
        }
    }

    private static string ColNumToLetter(int col)
    {
        var sb = new StringBuilder();
        while (col > 0)
        {
            col--;
            sb.Insert(0, (char)('A' + col % 26));
            col /= 26;
        }
        return sb.ToString();
    }

    private static void ConfigurarChartPartGrafico(ChartPart chartPart, List<GraficoDto> dados, string catFormula, string valFormula)
    {
        var chartSpace = new ChartSpace();
        chartSpace.AddNamespaceDeclaration("c", ExcelConstantes.OPENXML_FORMATS_CHART);
        chartSpace.AddNamespaceDeclaration("a", ExcelConstantes.OPENXML_FORMATS_MAIN);
        chartSpace.AddNamespaceDeclaration("r", ExcelConstantes.OPENXML_FORMATS_RELATIONSHIP);
        chartSpace.AppendChild(new RoundedCorners { Val = false });

        var chart = new Chart();
        chart.AppendChild(new AutoTitleDeleted { Val = true });

        var plotArea = new PlotArea();
        plotArea.AppendChild(CriarBarChart(dados, catFormula, valFormula));
        AdicionarEixos(plotArea);

        chart.AppendChild(plotArea);
        chart.AppendChild(new PlotVisibleOnly { Val = true });

        chartSpace.AppendChild(chart);
        chartPart.ChartSpace = chartSpace;
        chartPart.ChartSpace.Save();
    }

    private static DocumentFormat.OpenXml.Drawing.Charts.TextProperties CriarTextPropertiesGray()
    {
        var txPr = new DocumentFormat.OpenXml.Drawing.Charts.TextProperties();
        txPr.AppendChild(new DocumentFormat.OpenXml.Drawing.BodyProperties());
        txPr.AppendChild(new DocumentFormat.OpenXml.Drawing.ListStyle());
        var para = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var pPr = new DocumentFormat.OpenXml.Drawing.ParagraphProperties();
        var defRPr = new DocumentFormat.OpenXml.Drawing.DefaultRunProperties();
        var solidFill = new DocumentFormat.OpenXml.Drawing.SolidFill();
        solidFill.AppendChild(new DocumentFormat.OpenXml.Drawing.RgbColorModelHex { Val = "808080" });
        defRPr.AppendChild(solidFill);
        pPr.AppendChild(defRPr);
        para.AppendChild(pPr);
        txPr.AppendChild(para);
        return txPr;
    }

    private static BarChart CriarBarChart(List<GraficoDto> dados, string catFormula, string valFormula)
    {
        var barChart = new BarChart();
        barChart.AppendChild(new BarDirection { Val = BarDirectionValues.Column });
        barChart.AppendChild(new BarGrouping { Val = BarGroupingValues.Clustered });
        barChart.AppendChild(new VaryColors { Val = true });

        var serie = new BarChartSeries();
        serie.AppendChild(new Index { Val = 0 });
        serie.AppendChild(new Order { Val = 0 });

        AdicionarPontosDados(serie, dados);
        AdicionarDadosCategorias(serie, dados, catFormula);
        AdicionarDadosValores(serie, dados, valFormula);

        var dLbls = new DataLabels();
        dLbls.AppendChild(CriarTextPropertiesGray());
        dLbls.AppendChild(new ShowLegendKey { Val = false });
        dLbls.AppendChild(new ShowValue { Val = true });
        dLbls.AppendChild(new ShowCategoryName { Val = false });
        dLbls.AppendChild(new ShowSeriesName { Val = false });
        dLbls.AppendChild(new ShowPercent { Val = false });
        dLbls.AppendChild(new ShowBubbleSize { Val = false });
        serie.AppendChild(dLbls);

        barChart.AppendChild(serie);
        barChart.AppendChild(new AxisId { Val = 48650200U });
        barChart.AppendChild(new AxisId { Val = 48672800U });

        return barChart;
    }

    private static void AdicionarPontosDados(BarChartSeries serie, List<GraficoDto> dados)
    {
        for (int i = 0; i < dados.Count; i++)
        {
            var hex = (dados[i].Cor ?? "CCCCCC").TrimStart('#').ToUpper();
            if (hex.Length != 6) hex = "CCCCCC";

            var dp = new DataPoint();
            dp.AppendChild(new Index { Val = (uint)i });
            dp.AppendChild(new InvertIfNegative { Val = false });
            var spPr = new ChartShapeProperties();
            var solidFill = new DocumentFormat.OpenXml.Drawing.SolidFill();
            solidFill.AppendChild(new DocumentFormat.OpenXml.Drawing.RgbColorModelHex { Val = hex });
            spPr.AppendChild(solidFill);
            if (hex == "FFFFFF")
            {
                var outline = new DocumentFormat.OpenXml.Drawing.Outline();
                var outlineFill = new DocumentFormat.OpenXml.Drawing.SolidFill();
                outlineFill.AppendChild(new DocumentFormat.OpenXml.Drawing.RgbColorModelHex { Val = "000000" });
                outline.AppendChild(outlineFill);
                spPr.AppendChild(outline);
            }
            dp.AppendChild(spPr);
            serie.AppendChild(dp);
        }
    }

    private static void AdicionarDadosCategorias(BarChartSeries serie, List<GraficoDto> dados, string formula)
    {
        var catValues = new CategoryAxisData();
        var strRef = new StringReference();
        strRef.AppendChild(new ChartFormula(formula));
        var strCache = new StringCache();
        strCache.AppendChild(new PointCount { Val = (uint)dados.Count });
        for (int i = 0; i < dados.Count; i++)
            strCache.AppendChild(new StringPoint { Index = (uint)i, NumericValue = new NumericValue(dados[i].Descricao) });
        strRef.AppendChild(strCache);
        catValues.AppendChild(strRef);
        serie.AppendChild(catValues);
    }

    private static void AdicionarDadosValores(BarChartSeries serie, List<GraficoDto> dados, string formula)
    {
        var values = new Values();
        var numRef = new NumberReference();
        numRef.AppendChild(new ChartFormula(formula));
        var numCache = new NumberingCache();
        numCache.AppendChild(new FormatCode("General"));
        numCache.AppendChild(new PointCount { Val = (uint)dados.Count });
        for (int i = 0; i < dados.Count; i++)
            numCache.AppendChild(new NumericPoint { Index = (uint)i, NumericValue = new NumericValue(dados[i].Quantidade.ToString()) });
        numRef.AppendChild(numCache);
        values.AppendChild(numRef);
        serie.AppendChild(values);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("RoslynSquid", "S3220")]
    private static void AdicionarEixos(PlotArea plotArea)
    {
        var catAxis = new CategoryAxis();
        catAxis.Append(new List<OpenXmlElement> { new AxisId { Val = 48650200U } });
        catAxis.Append(new List<OpenXmlElement> { new Scaling(new Orientation { Val = OrientationValues.MinMax }) });
        catAxis.Append(new List<OpenXmlElement> { new Delete { Val = false } });
        catAxis.Append(new List<OpenXmlElement> { new AxisPosition { Val = AxisPositionValues.Bottom } });
        catAxis.Append(new List<OpenXmlElement> { new NumberingFormat { FormatCode = "General", SourceLinked = true } });
        catAxis.Append(new List<OpenXmlElement> { new MajorTickMark { Val = TickMarkValues.None } });
        catAxis.Append(new List<OpenXmlElement> { new MinorTickMark { Val = TickMarkValues.None } });
        catAxis.Append(new List<OpenXmlElement> { CriarTextPropertiesGray() });
        catAxis.Append(new List<OpenXmlElement> { new CrossingAxis { Val = 48672800U } });
        plotArea.Append(new List<OpenXmlElement> { catAxis });

        var valAxis = new ValueAxis();
        valAxis.Append(new List<OpenXmlElement> { new AxisId { Val = 48672800U } });
        valAxis.Append(new List<OpenXmlElement> { new Scaling(new Orientation { Val = OrientationValues.MinMax }) });
        valAxis.Append(new List<OpenXmlElement> { new Delete { Val = false } });
        valAxis.Append(new List<OpenXmlElement> { new AxisPosition { Val = AxisPositionValues.Left } });
        valAxis.Append(new List<OpenXmlElement> { new NumberingFormat { FormatCode = "General", SourceLinked = true } });
        valAxis.Append(new List<OpenXmlElement> { new MajorTickMark { Val = TickMarkValues.None } });
        valAxis.Append(new List<OpenXmlElement> { new MinorTickMark { Val = TickMarkValues.None } });
        valAxis.Append(new List<OpenXmlElement> { CriarTextPropertiesGray() });
        valAxis.Append(new List<OpenXmlElement> { new CrossingAxis { Val = 48650200U } });
        plotArea.Append(new List<OpenXmlElement> { valAxis });
    }

    private static Xdr.TwoCellAnchor CriarAnchorGrafico(string chartRelId, int linhaGrafico, uint graphicId, int fromCol, int toCol)
    {
        var anchor = new Xdr.TwoCellAnchor();
        anchor.AppendChild(new Xdr.FromMarker(
            new Xdr.ColumnId(fromCol.ToString()),
            new Xdr.ColumnOffset("0"),
            new Xdr.RowId((linhaGrafico - 1).ToString()),
            new Xdr.RowOffset("0")
        ));
        anchor.AppendChild(new Xdr.ToMarker(
            new Xdr.ColumnId(toCol.ToString()),
            new Xdr.ColumnOffset("0"),
            new Xdr.RowId((linhaGrafico + 19).ToString()),
            new Xdr.RowOffset("0")
        ));

        var graphicFrame = new Xdr.GraphicFrame { Macro = "" };
        graphicFrame.AppendChild(new Xdr.NonVisualGraphicFrameProperties(
            new Xdr.NonVisualDrawingProperties { Id = graphicId, Name = $"Grafico{graphicId}" },
            new Xdr.NonVisualGraphicFrameDrawingProperties()
        ));
        graphicFrame.AppendChild(new Xdr.Transform(
            new DocumentFormat.OpenXml.Drawing.Offset { X = 0L, Y = 0L },
            new DocumentFormat.OpenXml.Drawing.Extents { Cx = 0L, Cy = 0L }
        ));

        var graphic = new DocumentFormat.OpenXml.Drawing.Graphic();
        var graphicData = new DocumentFormat.OpenXml.Drawing.GraphicData { Uri = ExcelConstantes.OPENXML_FORMATS_CHART };
        graphicData.AppendChild(new ChartReference { Id = chartRelId });
        graphic.AppendChild(graphicData);
        graphicFrame.AppendChild(graphic);

        anchor.AppendChild(graphicFrame);
        anchor.AppendChild(new Xdr.ClientData());
        return anchor;
    }
}
