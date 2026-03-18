using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Index = DocumentFormat.OpenXml.Drawing.Charts.Index;
using NumberingFormat = DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat;
using OrientationValues = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues;
using Title = DocumentFormat.OpenXml.Drawing.Charts.Title;
using Values = DocumentFormat.OpenXml.Drawing.Charts.Values;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

[ExcludeFromCodeCoverage]
public  class RelatorioSondagemQuestionarioPorTurmaTemplateExcel : RelatorioTemplateBase, IRelatorioSondagemQuestionarioPorTurmaTemplateExcel
{
    public RelatorioSondagemQuestionarioPorTurmaTemplateExcel(IServicoArmazenamentoMinio servicoArmazenamentoMinio) : base(servicoArmazenamentoMinio)
    {
    }

    public async Task<string> GerarExcelEF(RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Sondagem");

        ConfigurarColunasExcel(sheet, relatorioSondagemPorTurmaDto);

        int linha = 1;

        ConfigurarCabecalho(sheet);

        sheet.AddPicture(ObterLogo())
            .MoveTo(sheet.Cell(1, 1))
            .WithSize(160, 60);

        linha = EscreverInformacoesCabecalhoRelatorio(sheet, relatorioSondagemPorTurmaDto, relatorioSondagemPorTurmaDto.Modalidade);

        linha = EscreverTitulo(sheet, "Relatório Sondagem", relatorioSondagemPorTurmaDto.Proficiencia, linha);

        linha = EscreverCabecalhoTabela(sheet, linha, relatorioSondagemPorTurmaDto);

        linha = EscreverDadosExcel(sheet, linha, relatorioSondagemPorTurmaDto);

        int linhaGrafico = linha + 6;

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        stream.Position = 0;

        var graficoCompleto = GerarDadosGrafico(relatorioSondagemPorTurmaDto);
        InjetarGraficoOpenXml(stream, graficoCompleto, relatorioSondagemPorTurmaDto.Proficiencia, linhaGrafico);

        stream.Position = 0;

        return await EnviarExcelParaMinio(stream, relatorioSondagemPorTurmaDto.CodigoCorrelacao);
    }

    private static void ConfigurarColunasExcel(IXLWorksheet sheet, RelatorioSondagemPorTurmaDto dados)
    {
        sheet.Column(1).Width = 10; // Nº Chamada
        sheet.Column(2).Width = 40; // Nome
        sheet.Column(3).Width = 12; // Raça
        sheet.Column(4).Width = 20; // Gênero

        int indiceColunaAtual = 5;

        if (dados.ExibeColunaLinguaPortuguesaSegundaLingua)
        {
            sheet.Column(indiceColunaAtual).Width = 20;
            indiceColunaAtual++;
        }

        var primeiroEstudante = dados.Estudantes.FirstOrDefault();

        if (primeiroEstudante != null && primeiroEstudante.Coluna != null)
        {
            foreach (var coluna in primeiroEstudante.Coluna)
            {
                sheet.Column(indiceColunaAtual).Width = 30;
                indiceColunaAtual++;
            }
        }
    }

    private static int EscreverCabecalhoTabela(IXLWorksheet sheet, int linha, RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto)
    {
        var headers = ObterHeaders(relatorioSondagemPorTurmaDto);

        int colunaInicialSondagem = relatorioSondagemPorTurmaDto.ExibeColunaLinguaPortuguesaSegundaLingua ? 4 : 5;
        int colunaFinalSondagem = headers.Max(h => h.Coluna);

        if (colunaFinalSondagem >= colunaInicialSondagem)
        {
            var grupo = sheet.Range(linha, colunaInicialSondagem, linha, colunaFinalSondagem);
            grupo.Merge();
            grupo.Value = relatorioSondagemPorTurmaDto.Proficiencia;
            grupo.Style.Font.Bold = true;
            grupo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            grupo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            grupo.Style.Fill.BackgroundColor = XLColor.LightGray;

            AplicarBordaExterna(grupo);
        }

        linha++;

        foreach (var (col, texto) in headers)
        {
            EstilizarHeader(sheet.Cell(linha, col), texto);
        }

        sheet.Row(linha).Height = 40;

        return linha + 1;
    }

    private static int EscreverDadosExcel(IXLWorksheet sheet, int linha, RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto)
    {
        if (relatorioSondagemPorTurmaDto.Estudantes == null) return linha;

        foreach (var aluno in relatorioSondagemPorTurmaDto.Estudantes)
        {
            sheet.Row(linha).Height = 45;

            // 1. Campos Fixos
            EstilarEPreencher(sheet.Cell(linha, 1), aluno.NumeroAlunoChamada);
            EstilarEPreencher(sheet.Cell(linha, 2), aluno.NomeRelatorio);
            EstilarEPreencher(sheet.Cell(linha, 3), aluno.Raca);
            EstilarEPreencher(sheet.Cell(linha, 4), aluno.Genero);

            int colunaAtual = 5;

            if (relatorioSondagemPorTurmaDto.ExibeColunaLinguaPortuguesaSegundaLingua)
            {
                EscreverCelulaLP(sheet.Cell(linha, colunaAtual), aluno.LinguaPortuguesaSegundaLingua);
                colunaAtual++;
            }

            if (aluno.Coluna != null)
            {
                foreach (var colunaSondagem in aluno.Coluna)
                {
                    var resposta = colunaSondagem.OpcaoResposta?.FirstOrDefault(o => o.Id == colunaSondagem.Resposta?.OpcaoRespostaId);
                    var corHex = !string.IsNullOrEmpty(resposta?.CorFundo) ? resposta.CorFundo : "#333333";
                    var corFundo = ConverterCor(corHex);

                    PreencherCelulaSondagem(sheet.Cell(linha, colunaAtual), resposta?.DescricaoOpcaoResposta ?? "Vazio", corFundo);
                    colunaAtual++;
                }
            }

            linha++;
        }

        return linha;
    }

    private static (int Coluna, string Texto)[] ObterHeaders(RelatorioSondagemPorTurmaDto dados)
    {
        var headers = new List<(int Coluna, string Texto)>();

        headers.Add((1, "Nº"));
        headers.Add((2, "Nome"));
        headers.Add((3, "Raça"));
        headers.Add((4, "Gênero"));

        int indiceAtual = 5;

        if (dados.ExibeColunaLinguaPortuguesaSegundaLingua)
        {
            headers.Add((indiceAtual, "LP como 2ª língua?"));
            indiceAtual++;
        }

        var primeiroEstudante = dados.Estudantes.FirstOrDefault();
        if (primeiroEstudante != null)
        {
            foreach (var coluna in primeiroEstudante.Coluna)
            {
                headers.Add((indiceAtual, coluna.DescricaoColuna));
                indiceAtual++;
            }
        }

        return headers.ToArray();
    }

    private static void InjetarGraficoOpenXml(Stream stream, List<GraficoDto> dados, string tituloProficiencia, int linhaGrafico)
    {
        using var document = SpreadsheetDocument.Open(stream, true);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("WorkbookPart nulo.");
        var worksheetPart = ObterWorksheetPart(workbookPart);

        var drawingsPart = PrepararDrawingsPart(worksheetPart);
        var chartPart = drawingsPart.AddNewPart<ChartPart>();

        ConfigurarChartPart(chartPart, dados, tituloProficiencia);

        AncorarGrafico(drawingsPart, chartPart, worksheetPart, linhaGrafico);
    }

    private static WorksheetPart ObterWorksheetPart(WorkbookPart workbookPart)
    {
        var workbook = workbookPart.Workbook ?? throw new InvalidOperationException("Workbook nulo.");
        var sheets = workbook.Sheets ?? throw new InvalidOperationException("Sheets nulo.");
        var sheetInfo = sheets.Elements<Sheet>().First();
        return (WorksheetPart)workbookPart.GetPartById(sheetInfo.Id?.Value ?? string.Empty);
    }

    private static DrawingsPart PrepararDrawingsPart(WorksheetPart worksheetPart)
    {
        return worksheetPart.DrawingsPart ?? worksheetPart.AddNewPart<DrawingsPart>();
    }

    private static void ConfigurarChartPart(ChartPart chartPart, List<GraficoDto> dados, string tituloProficiencia)
    {
        var chartSpace = new ChartSpace();
        AdicionarNamespaces(chartSpace);

        var chart = new Chart();
        chart.Append(new AutoTitleDeleted { Val = false });

        AdicionarTituloGrafico(chart, tituloProficiencia);

        var plotArea = new PlotArea();
        var barChart = CriarBarChart(dados, tituloProficiencia);
        plotArea.Append(barChart);

        AdicionarEixos(plotArea);

        chart.Append(plotArea);
        chart.Append(new PlotVisibleOnly { Val = true });
        chartSpace.Append(chart);

        chartPart.ChartSpace = chartSpace;
        chartPart.ChartSpace.Save();
    }

    private static void AdicionarNamespaces(ChartSpace chartSpace)
    {
        chartSpace.AddNamespaceDeclaration("c", "http://schemas.openxmlformats.org/drawingml/2006/chart");
        chartSpace.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
        chartSpace.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
    }

    private static void AdicionarTituloGrafico(Chart chart, string tituloProficiencia)
    {
        var chartTitle = new Title();
        var chartText = new ChartText();
        var richText = new RichText();
        richText.Append(new DocumentFormat.OpenXml.Drawing.BodyProperties());
        richText.Append(new DocumentFormat.OpenXml.Drawing.ListStyle());

        richText.Append(CriarParagrafoTitulo("Gráfico da Sondagem", 1400, true));
        richText.Append(CriarParagrafoTitulo(tituloProficiencia, 1100, true));

        chartText.Append(richText);
        chartTitle.Append(chartText);
        chartTitle.Append(new Overlay { Val = false });
        chart.Append(chartTitle);
    }

    private static DocumentFormat.OpenXml.Drawing.Paragraph CriarParagrafoTitulo(string texto, int fontSize, bool isBold)
    {
        var paragraph = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var run = new DocumentFormat.OpenXml.Drawing.Run();
        run.Append(new DocumentFormat.OpenXml.Drawing.RunProperties { Bold = isBold, FontSize = fontSize });
        run.Append(new DocumentFormat.OpenXml.Drawing.Text(texto));
        paragraph.Append(run);
        return paragraph;
    }

    private static BarChart CriarBarChart(List<GraficoDto> dados, string tituloProficiencia)
    {
        var barChart = new BarChart();
        barChart.Append(new BarDirection { Val = BarDirectionValues.Column });
        barChart.Append(new BarGrouping { Val = BarGroupingValues.Clustered });
        barChart.Append(new VaryColors { Val = false });

        var serie = CriarSerieBarra(dados, tituloProficiencia);
        barChart.Append(serie);

        barChart.Append(new AxisId { Val = 48650112U });
        barChart.Append(new AxisId { Val = 48672768U });

        return barChart;
    }

    private static BarChartSeries CriarSerieBarra(List<GraficoDto> dados, string tituloProficiencia)
    {
        var serie = new BarChartSeries();
        serie.Append(new Index { Val = 0 });
        serie.Append(new Order { Val = 0 });

        var serTitle = new SeriesText();
        serTitle.Append(new NumericValue(tituloProficiencia));
        serie.Append(serTitle);

        AdicionarPontosDados(serie, dados);
        AdicionarDadosCategorias(serie, dados);
        AdicionarDadosValores(serie, dados);

        var dLbls = new DataLabels();
        dLbls.Append(new ShowLegendKey { Val = false });
        dLbls.Append(new ShowValue { Val = true });
        dLbls.Append(new ShowCategoryName { Val = false });
        dLbls.Append(new ShowSeriesName { Val = false });
        dLbls.Append(new ShowPercent { Val = false });
        dLbls.Append(new ShowBubbleSize { Val = false });
        serie.Append(dLbls);

        return serie;
    }

    private static void AdicionarPontosDados(BarChartSeries serie, List<GraficoDto> dados)
    {
        for (int i = 0; i < dados.Count; i++)
        {
            var hex = (dados[i].Cor ?? "CCCCCC").TrimStart('#').ToUpper();
            if (hex.Length != 6) hex = "CCCCCC";

            var dp = new DataPoint();
            dp.Append(new Index { Val = (uint)i });
            dp.Append(new InvertIfNegative { Val = false });
            var spPr = new ChartShapeProperties();
            var solidFill = new DocumentFormat.OpenXml.Drawing.SolidFill();
            solidFill.Append(new DocumentFormat.OpenXml.Drawing.RgbColorModelHex { Val = hex });
            spPr.Append(solidFill);
            dp.Append(spPr);
            serie.Append(dp);
        }
    }

    private static void AdicionarDadosCategorias(BarChartSeries serie, List<GraficoDto> dados)
    {
        var catValues = new CategoryAxisData();
        var strRef = new StringReference();
        var strCache = new StringCache();
        strCache.Append(new PointCount { Val = (uint)dados.Count });
        for (int i = 0; i < dados.Count; i++)
            strCache.Append(new StringPoint { Index = (uint)i, NumericValue = new NumericValue(dados[i].Descricao) });
        strRef.Append(strCache);
        catValues.Append(strRef);
        serie.Append(catValues);
    }

    private static void AdicionarDadosValores(BarChartSeries serie, List<GraficoDto> dados)
    {
        var values = new Values();
        var numRef = new NumberReference();
        var numCache = new NumberingCache();

        numCache.Append(new FormatCode("General"));
        numCache.Append(new PointCount { Val = (uint)dados.Count });

        for (int i = 0; i < dados.Count; i++)
        {
            numCache.Append(new NumericPoint
            {
                Index = (uint)i,
                NumericValue = new NumericValue(dados[i].Quantidade.ToString())
            });
        }

        numRef.AppendChild(numCache);
        values.AppendChild(numRef);
        serie.AppendChild(values);
    }

    private static void AdicionarEixos(PlotArea plotArea)
    {
        var catAxis = new CategoryAxis();
        catAxis.Append(new AxisId { Val = 48650112U });
        catAxis.Append(new Scaling(new Orientation { Val = OrientationValues.MinMax }));
        catAxis.Append(new Delete { Val = false });
        catAxis.Append(new AxisPosition { Val = AxisPositionValues.Bottom });
        catAxis.Append(new NumberingFormat { FormatCode = "General", SourceLinked = true });
        catAxis.Append(new MajorTickMark { Val = TickMarkValues.None });
        catAxis.Append(new MinorTickMark { Val = TickMarkValues.None });

        ConfigurarTituloEixo(catAxis, "Opções de respostas");

        catAxis.Append(new CrossingAxis { Val = 48672768U });
        plotArea.Append(catAxis);

        var valAxis = new ValueAxis();
        valAxis.Append(new AxisId { Val = 48672768U });
        valAxis.Append(new Scaling(new Orientation { Val = OrientationValues.MinMax }));
        valAxis.Append(new Delete { Val = false });
        valAxis.Append(new AxisPosition { Val = AxisPositionValues.Left });
        valAxis.Append(new NumberingFormat { FormatCode = "General", SourceLinked = true });
        valAxis.Append(new MajorTickMark { Val = TickMarkValues.None });
        valAxis.Append(new MinorTickMark { Val = TickMarkValues.None });

        ConfigurarTituloEixo(valAxis, "Quantidade de estudantes");

        valAxis.Append(new CrossingAxis { Val = 48650112U });
        plotArea.Append(valAxis);
    }

    private static void ConfigurarTituloEixo(OpenXmlCompositeElement axis, string titulo)
    {
        var axisTitle = new Title();
        var axisChartText = new ChartText();
        var axisRichText = new RichText();
        axisRichText.Append(new DocumentFormat.OpenXml.Drawing.BodyProperties());
        axisRichText.Append(new DocumentFormat.OpenXml.Drawing.ListStyle());
        var axisPara = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var axisRun = new DocumentFormat.OpenXml.Drawing.Run();
        axisRun.Append(new DocumentFormat.OpenXml.Drawing.RunProperties { Bold = true, FontSize = 1000 });
        axisRun.Append(new DocumentFormat.OpenXml.Drawing.Text(titulo));
        axisPara.Append(axisRun);
        axisRichText.Append(axisPara);
        axisChartText.Append(axisRichText);
        axisTitle.Append(axisChartText);
        axisTitle.Append(new Overlay { Val = false });
        axis.Append(axisTitle);
    }

    private static void AncorarGrafico(DrawingsPart drawingsPart, ChartPart chartPart, WorksheetPart worksheetPart, int linhaGrafico)
    {
        var wsDr = drawingsPart.WorksheetDrawing ?? new Xdr.WorksheetDrawing();
        GarantirNamespacesDesenho(wsDr);

        var chartRelId = drawingsPart.GetIdOfPart(chartPart);
        var twoCellAnchor = CriarAnchor(chartRelId, linhaGrafico, drawingsPart.Parts.Count());

        wsDr.Append(twoCellAnchor);

        if (drawingsPart.WorksheetDrawing == null)
            drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing(wsDr);
        else
            drawingsPart.WorksheetDrawing.Save();

        VincularDesenhoAoWorksheet(worksheetPart, drawingsPart);
        worksheetPart.Worksheet.Save();
    }

    private static void GarantirNamespacesDesenho(Xdr.WorksheetDrawing wsDr)
    {
        if (wsDr.LookupNamespace("xdr") == null)
            wsDr.AddNamespaceDeclaration("xdr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
        if (wsDr.LookupNamespace("a") == null)
            wsDr.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
        if (wsDr.LookupNamespace("r") == null)
            wsDr.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
    }

    private static Xdr.TwoCellAnchor CriarAnchor(string chartRelId, int linhaGrafico, int partsCount)
    {
        var twoCellAnchor = new Xdr.TwoCellAnchor();
        twoCellAnchor.Append(new Xdr.FromMarker(
            new Xdr.ColumnId("0"),
            new Xdr.ColumnOffset("0"),
            new Xdr.RowId((linhaGrafico - 1).ToString()),
            new Xdr.RowOffset("0")
        ));
        twoCellAnchor.Append(new Xdr.ToMarker(
            new Xdr.ColumnId("9"),
            new Xdr.ColumnOffset("0"),
            new Xdr.RowId((linhaGrafico + 19).ToString()),
            new Xdr.RowOffset("0")
        ));

        var graphicFrame = new Xdr.GraphicFrame { Macro = "" };
        graphicFrame.Append(new Xdr.NonVisualGraphicFrameProperties(
            new Xdr.NonVisualDrawingProperties { Id = (uint)(partsCount + 10), Name = "GraficoSondagem" },
            new Xdr.NonVisualGraphicFrameDrawingProperties()
        ));
        graphicFrame.Append(new Xdr.Transform(
            new DocumentFormat.OpenXml.Drawing.Offset { X = 0L, Y = 0L },
            new DocumentFormat.OpenXml.Drawing.Extents { Cx = 0L, Cy = 0L }
        ));

        var graphic = new DocumentFormat.OpenXml.Drawing.Graphic();
        var graphicData = new DocumentFormat.OpenXml.Drawing.GraphicData { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" };
        graphicData.Append(new ChartReference { Id = chartRelId });
        graphic.Append(graphicData);
        graphicFrame.Append(graphic);

        twoCellAnchor.Append(graphicFrame);
        twoCellAnchor.Append(new Xdr.ClientData());
        return twoCellAnchor;
    }

    private static void VincularDesenhoAoWorksheet(WorksheetPart worksheetPart, DrawingsPart drawingsPart)
    {
        if (!worksheetPart.Worksheet.Elements<Drawing>().Any())
        {
            var drawingRelId = worksheetPart.GetIdOfPart(drawingsPart);
            worksheetPart.Worksheet.Append(new Drawing { Id = drawingRelId });
        }
    }


    private static int EscreverInformacoesCabecalhoRelatorio(IXLWorksheet sheet, RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto, Modalidade modalidade)
    {
        int linha = 4;
        var semestreBimestre = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(relatorioSondagemPorTurmaDto.Bimestre, relatorioSondagemPorTurmaDto.Semestre, modalidade);

        EscreverCelula(sheet, linha, 1, $"Ano letivo: {relatorioSondagemPorTurmaDto.AnoLetivo}");
        sheet.Range(linha, 1, linha, 3).Merge();

        EscreverCelula(sheet, linha, 4, $"Modalidade: {relatorioSondagemPorTurmaDto.Modalidade}");
        sheet.Range(linha, 4, linha, 6).Merge();

        EscreverCelula(sheet, linha, 7, $"Dre: {relatorioSondagemPorTurmaDto.SiglaDre}");
        sheet.Range(linha, 7, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Unidade Educacional: {relatorioSondagemPorTurmaDto.UnidadeEducacional}");
        sheet.Range(linha, 1, linha, 6).Merge();

        EscreverCelula(sheet, linha, 7, $"Turma: {relatorioSondagemPorTurmaDto.Turma}");
        sheet.Range(linha, 7, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Proficiência: {relatorioSondagemPorTurmaDto.Proficiencia}");
        sheet.Range(linha, 1, linha, 5).Merge();

        EscreverCelula(sheet, linha, 6, $"{semestreBimestre.NomeFiltro}: {semestreBimestre.ValorFiltro}");
        sheet.Range(linha, 6, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Usuário: {relatorioSondagemPorTurmaDto.Usuario}");
        sheet.Range(linha, 1, linha, 5).Merge();

        EscreverCelula(sheet, linha, 6, $"Data de impressão: {relatorioSondagemPorTurmaDto.DataImpressao}");
        sheet.Range(linha, 6, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;
        linha++;

        return linha;
    }
}
