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
using Index = DocumentFormat.OpenXml.Drawing.Charts.Index;
using NumberingFormat = DocumentFormat.OpenXml.Drawing.Charts.NumberingFormat;
using OrientationValues = DocumentFormat.OpenXml.Drawing.Charts.OrientationValues;
using Title = DocumentFormat.OpenXml.Drawing.Charts.Title;
using Values = DocumentFormat.OpenXml.Drawing.Charts.Values;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaExcel : IRelatorioSondagemQuestionarioPorTurmaExcel
{
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;

    public RelatorioSondagemQuestionarioPorTurmaExcel(
        IServicoArmazenamentoMinio servicoArmazenamentoMinio)
    {
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
    }

    public async Task<string> GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(
        ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao)
    {
        var dto = consultaSondagemPorTurmaDto.MapToEscritaEfTurmaSondagemCabecalhoExcelDto(
            consultaSondagemPorTurmaDto.AnoLetivo,
            consultaSondagemPorTurmaDto.Turma,
            consultaSondagemPorTurmaDto.UnidadeEducacional,
            consultaSondagemPorTurmaDto?.SiglaDre,
            consultaSondagemPorTurmaDto?.Modalidade.ShortName(),
            consultaSondagemPorTurmaDto?.Usuario);

        return await GerarExcelEF(dto, consultaSondagemPorTurmaDto.Modalidade, codigoCorrelacao, consultaSondagemPorTurmaDto);
    }

    private async Task<string> GerarExcelEF(EscritaEfTurmaSondagemCabecalhoExcelDto dto, Modalidade modalidade, Guid codigoCorrelacao, ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Sondagem");

        ConfigurarColunasExcel(sheet, consultaSondagemPorTurmaDto);

        var graficoCompleto = GerarDadosGrafico(dto, modalidade);

        int linha = 1;

        ConfigurarCabecalho(sheet);

        sheet.AddPicture(ObterLogo())
            .MoveTo(sheet.Cell(1, 1))
            .WithSize(160, 60);

        linha = EscreverInformacoesCabecalhoRelatorio(sheet, dto);

        linha = EscreverTitulo(sheet, dto, linha);

        linha = EscreverCabecalhoTabela(sheet, dto, modalidade, linha, consultaSondagemPorTurmaDto);

        linha = EscreverDadosExcel(sheet, dto, modalidade, linha, consultaSondagemPorTurmaDto);

        int linhaGrafico = linha + 6;

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        stream.Position = 0;

        InjetarGraficoOpenXml(stream, graficoCompleto, dto.Proeficiencia, linhaGrafico);

        stream.Position = 0;

        return await EnviarExcelParaMinio(stream, codigoCorrelacao);
    }

    private static void ConfigurarColunasExcel(IXLWorksheet sheet, ConsultaSondagemPorTurmaDto dados)
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

    private static int EscreverTitulo(IXLWorksheet sheet, EscritaEfTurmaSondagemCabecalhoExcelDto dto, int linha)
    {
        var titulo = sheet.Cell(linha, 1);
        titulo.Value = "Relatório da Sondagem";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 14;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Range(linha, 1, linha, 10).Merge();
        linha++;

        var subtitulo = sheet.Cell(linha, 1);
        subtitulo.Value = dto.Proeficiencia;
        subtitulo.Style.Font.Bold = true;
        subtitulo.Style.Font.FontSize = 12;
        subtitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        sheet.Range(linha, 1, linha, 10).Merge();
        linha += 2;

        return linha;
    }

    private static int EscreverCabecalhoTabela(IXLWorksheet sheet, EscritaEfTurmaSondagemCabecalhoExcelDto dto, Modalidade modalidade, int linha, ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto)
    {
        var headers = ObterHeaders(consultaSondagemPorTurmaDto);

        int colunaInicialSondagem = consultaSondagemPorTurmaDto.ExibeColunaLinguaPortuguesaSegundaLingua ? 4 : 5;
        int colunaFinalSondagem = headers.Max(h => h.Coluna);

        if (colunaFinalSondagem >= colunaInicialSondagem)
        {
            var grupo = sheet.Range(linha, colunaInicialSondagem, linha, colunaFinalSondagem);
            grupo.Merge();
            grupo.Value = dto.Proeficiencia;
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

    private static int EscreverDadosExcel(IXLWorksheet sheet, EscritaEfTurmaSondagemCabecalhoExcelDto dto, Modalidade modalidade, int linha, ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto)
    {
        foreach (var item in dto.CorpoRelatorio)
        {
            sheet.Row(linha).Height = 45;

            // 1. Campos Fixos
            EstilarEPreencher(sheet.Cell(linha, 1), item.Numero);
            EstilarEPreencher(sheet.Cell(linha, 2), item.Nome);
            EstilarEPreencher(sheet.Cell(linha, 3), item.Raca);
            EstilarEPreencher(sheet.Cell(linha, 4), item.Genero);

            int colunaAtual = 5;

            if (consultaSondagemPorTurmaDto.ExibeColunaLinguaPortuguesaSegundaLingua)
            {
                var cLp = sheet.Cell(linha, colunaAtual);
                cLp.Value = item.LpComoLinguaPrincipal == "Sim" ? "☑" : "☐";
                cLp.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cLp.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cLp.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cLp.Style.Font.FontSize = 14;
                colunaAtual++;
            }

            var estudanteSondagem = consultaSondagemPorTurmaDto.Estudantes
                .FirstOrDefault(e => e.NomeRelatorio == item.Nome);

            if (estudanteSondagem != null)
            {
                foreach (var colunaSondagem in estudanteSondagem.Coluna)
                {
                    var resposta = colunaSondagem.OpcaoResposta
                        .FirstOrDefault(o => o.Id == colunaSondagem.Resposta?.OpcaoRespostaId);

                    var corHex = !string.IsNullOrEmpty(resposta?.CorFundo) ? resposta.CorFundo : item.Cor;
                    var corFundo = ConverterCor(corHex);

                    PreencherCelulaSondagem(sheet.Cell(linha, colunaAtual), resposta?.DescricaoOpcaoResposta ?? "Vazio", corFundo);

                    colunaAtual++;
                }
            }

            linha++;
        }

        return linha;
    }

    private static void EstilarEPreencher(IXLCell celula, object valor)
    {
        celula.Value = valor?.ToString();
        EstilarCelulaDados(celula);
    }

    private static (int Coluna, string Texto)[] ObterHeaders(ConsultaSondagemPorTurmaDto dados)
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

    private List<GraficoDto> GerarDadosGrafico(
        EscritaEfTurmaSondagemCabecalhoExcelDto dto,
        Modalidade modalidade)
    {
        var seletores = new List<Func<EscritaEfTurmaSondagemCorpoExcelDto, string>>
        {
            x => x.SondagemInicial,
            x => x.PrimeiroBimestre,
            x => x.SegundoBimestre
        };

        if (modalidade == Modalidade.Fundamental)
        {
            seletores.Add(x => x.TerceiroBimestre);
            seletores.Add(x => x.QuartoBimestre);
        }

        return seletores
            .SelectMany(s => ContarOcorrencias(dto.CorpoRelatorio, s))
            .GroupBy(x => x.Descricao)
            .Select(g => new GraficoDto
            {
                Descricao = g.Key,
                Quantidade = g.Sum(x => x.Quantidade),
                Cor = g.First().Cor
            })
            .ToList();
    }

    private List<GraficoDto> ContarOcorrencias(
        List<EscritaEfTurmaSondagemCorpoExcelDto> corpo,
        Func<EscritaEfTurmaSondagemCorpoExcelDto, string> seletor)
    {
        return corpo
            .Where(x => !string.IsNullOrEmpty(seletor(x)))
            .GroupBy(seletor)
            .Select(g => new GraficoDto
            {
                Descricao = g.Key,
                Quantidade = g.Count(),
                Cor = g.First().Cor
            })
            .ToList();
    }

    private static XLColor ConverterCor(string cor)
    {
        if (string.IsNullOrWhiteSpace(cor))
            return XLColor.White;

        var hex = cor.TrimStart('#');

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return XLColor.FromArgb(r, g, b);
    }

    static void PreencherCelulaSondagem(IXLCell cell, string valor, XLColor corFundo)
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

    static void EstilizarHeader(IXLCell cell, string texto)
    {
        cell.Value = texto;
        cell.Style.Font.Bold = true;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    static void EscreverCelula(IXLWorksheet ws, int row, int col, string valor, bool bold = false)
    {
        var cell = ws.Cell(row, col);
        cell.Value = valor;
        cell.Style.Font.Bold = bold;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
    }

    static void AplicarBordaExterna(IXLRange range)
    {
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    static void EstilarCelulaDados(IXLCell cell)
    {
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static Stream ObterLogo()
    {
        string base64Logo = SmeConstants.LogoSmeMono.Substring(SmeConstants.LogoSmeMono.IndexOf(',') + 1);
        return new MemoryStream(Convert.FromBase64String(base64Logo));
    }

    private async Task<string> EnviarExcelParaMinio(Stream stream, Guid codigoCorrelacao)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var excelBytes = ms.ToArray();

        string nomeArquivo = $"Relatorio/{codigoCorrelacao}.xlsx";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(
            excelBytes,
            nomeArquivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }

    private void InjetarGraficoOpenXml(Stream stream, List<GraficoDto> dados, string tituloProficiencia, int linhaGrafico)
    {
        using var document = SpreadsheetDocument.Open(stream, true);
        var workbookPart = document.WorkbookPart;

        var sheetInfo = workbookPart.Workbook.Sheets.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetInfo.Id);

        DrawingsPart drawingsPart;
        if (worksheetPart.DrawingsPart != null)
            drawingsPart = worksheetPart.DrawingsPart;
        else
            drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();

        var chartPart = drawingsPart.AddNewPart<ChartPart>();

        var chartSpace = new ChartSpace();
        chartSpace.AddNamespaceDeclaration("c", "http://schemas.openxmlformats.org/drawingml/2006/chart");
        chartSpace.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
        chartSpace.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var chart = new Chart();
        chart.Append(new AutoTitleDeleted() { Val = false });

        var chartTitle = new Title();
        var chartText = new ChartText();
        var richText = new RichText();
        richText.Append(new DocumentFormat.OpenXml.Drawing.BodyProperties());
        richText.Append(new DocumentFormat.OpenXml.Drawing.ListStyle());

        var titlePara1 = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var titleRun1 = new DocumentFormat.OpenXml.Drawing.Run();
        titleRun1.Append(new DocumentFormat.OpenXml.Drawing.RunProperties() { Bold = true, FontSize = 1400 });
        titleRun1.Append(new DocumentFormat.OpenXml.Drawing.Text("Gráfico da Sondagem"));
        titlePara1.Append(titleRun1);
        richText.Append(titlePara1);

        var titlePara2 = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var titleRun2 = new DocumentFormat.OpenXml.Drawing.Run();
        titleRun2.Append(new DocumentFormat.OpenXml.Drawing.RunProperties() { Bold = true, FontSize = 1100 });
        titleRun2.Append(new DocumentFormat.OpenXml.Drawing.Text(tituloProficiencia));
        titlePara2.Append(titleRun2);
        richText.Append(titlePara2);

        chartText.Append(richText);
        chartTitle.Append(chartText);
        chartTitle.Append(new Overlay() { Val = false });
        chart.Append(chartTitle);

        var plotArea = new PlotArea();

        var barChart = new BarChart();
        barChart.Append(new BarDirection() { Val = BarDirectionValues.Column });
        barChart.Append(new BarGrouping() { Val = BarGroupingValues.Clustered });
        barChart.Append(new VaryColors() { Val = false });

        var serie = new BarChartSeries();
        serie.Append(new Index() { Val = 0 });
        serie.Append(new Order() { Val = 0 });

        var serTitle = new SeriesText();
        serTitle.Append(new NumericValue(tituloProficiencia));
        serie.Append(serTitle);

        for (int i = 0; i < dados.Count; i++)
        {
            var hex = (dados[i].Cor ?? "CCCCCC").TrimStart('#').ToUpper();
            if (hex.Length != 6) hex = "CCCCCC";

            var dp = new DataPoint();
            dp.Append(new Index() { Val = (uint)i });
            dp.Append(new InvertIfNegative() { Val = false });
            var spPr = new ChartShapeProperties();
            var solidFill = new DocumentFormat.OpenXml.Drawing.SolidFill();
            solidFill.Append(new DocumentFormat.OpenXml.Drawing.RgbColorModelHex() { Val = hex });
            spPr.Append(solidFill);
            dp.Append(spPr);
            serie.Append(dp);
        }

        var catValues = new CategoryAxisData();
        var strRef = new StringReference();
        var strCache = new StringCache();
        strCache.Append(new PointCount() { Val = (uint)dados.Count });
        for (int i = 0; i < dados.Count; i++)
            strCache.Append(new StringPoint() { Index = (uint)i, NumericValue = new NumericValue(dados[i].Descricao) });
        strRef.Append(strCache);
        catValues.Append(strRef);
        serie.Append(catValues);

        var values = new Values();
        var numRef = new NumberReference();
        var numCache = new NumberingCache();
        numCache.Append(new FormatCode("General"));
        numCache.Append(new PointCount() { Val = (uint)dados.Count });
        for (int i = 0; i < dados.Count; i++)
            numCache.Append(new NumericPoint() { Index = (uint)i, NumericValue = new NumericValue(dados[i].Quantidade.ToString()) });
        numRef.Append(numCache);
        values.Append(numRef);
        serie.Append(values);

        var dLbls = new DataLabels();
        dLbls.Append(new ShowLegendKey() { Val = false });
        dLbls.Append(new ShowValue() { Val = true });
        dLbls.Append(new ShowCategoryName() { Val = false });
        dLbls.Append(new ShowSeriesName() { Val = false });
        dLbls.Append(new ShowPercent() { Val = false });
        dLbls.Append(new ShowBubbleSize() { Val = false });
        serie.Append(dLbls);

        barChart.Append(serie);
        barChart.Append(new AxisId() { Val = 48650112U });
        barChart.Append(new AxisId() { Val = 48672768U });
        plotArea.Append(barChart);

        var catAxis = new CategoryAxis();
        catAxis.Append(new AxisId() { Val = 48650112U });
        catAxis.Append(new Scaling(new Orientation() { Val = OrientationValues.MinMax }));
        catAxis.Append(new Delete() { Val = false });
        catAxis.Append(new AxisPosition() { Val = AxisPositionValues.Bottom });
        catAxis.Append(new NumberingFormat() { FormatCode = "General", SourceLinked = true });
        catAxis.Append(new MajorTickMark() { Val = TickMarkValues.None });
        catAxis.Append(new MinorTickMark() { Val = TickMarkValues.None });

        var catAxisTitle = new Title();
        var catAxisChartText = new ChartText();
        var catAxisRichText = new RichText();
        catAxisRichText.Append(new DocumentFormat.OpenXml.Drawing.BodyProperties());
        catAxisRichText.Append(new DocumentFormat.OpenXml.Drawing.ListStyle());
        var catAxisPara = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var catAxisRun = new DocumentFormat.OpenXml.Drawing.Run();
        catAxisRun.Append(new DocumentFormat.OpenXml.Drawing.RunProperties() { Bold = true, FontSize = 1000 });
        catAxisRun.Append(new DocumentFormat.OpenXml.Drawing.Text("Opções de respostas"));
        catAxisPara.Append(catAxisRun);
        catAxisRichText.Append(catAxisPara);
        catAxisChartText.Append(catAxisRichText);
        catAxisTitle.Append(catAxisChartText);
        catAxisTitle.Append(new Overlay() { Val = false });
        catAxis.Append(catAxisTitle);

        catAxis.Append(new CrossingAxis() { Val = 48672768U });
        plotArea.Append(catAxis);

        var valAxis = new ValueAxis();
        valAxis.Append(new AxisId() { Val = 48672768U });
        valAxis.Append(new Scaling(new Orientation() { Val = OrientationValues.MinMax }));
        valAxis.Append(new Delete() { Val = false });
        valAxis.Append(new AxisPosition() { Val = AxisPositionValues.Left });
        valAxis.Append(new NumberingFormat() { FormatCode = "General", SourceLinked = true });
        valAxis.Append(new MajorTickMark() { Val = TickMarkValues.None });
        valAxis.Append(new MinorTickMark() { Val = TickMarkValues.None });

        var valAxisTitle = new Title();
        var valAxisChartText = new ChartText();
        var valAxisRichText = new RichText();
        var valAxisBodyProps = new DocumentFormat.OpenXml.Drawing.BodyProperties();
        valAxisRichText.Append(valAxisBodyProps);
        valAxisRichText.Append(new DocumentFormat.OpenXml.Drawing.ListStyle());
        var valAxisPara = new DocumentFormat.OpenXml.Drawing.Paragraph();
        var valAxisRun = new DocumentFormat.OpenXml.Drawing.Run();
        valAxisRun.Append(new DocumentFormat.OpenXml.Drawing.RunProperties() { Bold = true, FontSize = 1000 });
        valAxisRun.Append(new DocumentFormat.OpenXml.Drawing.Text("Quantidade de estudantes"));
        valAxisPara.Append(valAxisRun);
        valAxisRichText.Append(valAxisPara);
        valAxisChartText.Append(valAxisRichText);
        valAxisTitle.Append(valAxisChartText);
        valAxisTitle.Append(new Overlay() { Val = false });
        valAxis.Append(valAxisTitle);

        valAxis.Append(new CrossingAxis() { Val = 48650112U });
        plotArea.Append(valAxis);

        chart.Append(plotArea);
        chart.Append(new PlotVisibleOnly() { Val = true });
        chartSpace.Append(chart);

        chartPart.ChartSpace = chartSpace;
        chartPart.ChartSpace.Save();

        var wsDr = drawingsPart.WorksheetDrawing ?? new Xdr.WorksheetDrawing();

        if (wsDr.LookupNamespace("xdr") == null)
            wsDr.AddNamespaceDeclaration("xdr", "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing");
        if (wsDr.LookupNamespace("a") == null)
            wsDr.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");
        if (wsDr.LookupNamespace("r") == null)
            wsDr.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var chartRelId = drawingsPart.GetIdOfPart(chartPart);

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

        var graphicFrame = new Xdr.GraphicFrame() { Macro = "" };
        graphicFrame.Append(new Xdr.NonVisualGraphicFrameProperties(
            new Xdr.NonVisualDrawingProperties() { Id = (uint)(drawingsPart.Parts.Count() + 10), Name = "GraficoSondagem" },
            new Xdr.NonVisualGraphicFrameDrawingProperties()
        ));
        graphicFrame.Append(new Xdr.Transform(
            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
            new DocumentFormat.OpenXml.Drawing.Extents() { Cx = 0L, Cy = 0L }
        ));

        var graphic = new DocumentFormat.OpenXml.Drawing.Graphic();
        var graphicData = new DocumentFormat.OpenXml.Drawing.GraphicData()
        {
            Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart"
        };
        graphicData.Append(new ChartReference() { Id = chartRelId });
        graphic.Append(graphicData);
        graphicFrame.Append(graphic);

        twoCellAnchor.Append(graphicFrame);
        twoCellAnchor.Append(new Xdr.ClientData());
        wsDr.Append(twoCellAnchor);

        if (drawingsPart.WorksheetDrawing == null)
            drawingsPart.WorksheetDrawing = new Xdr.WorksheetDrawing(wsDr);
        else
            drawingsPart.WorksheetDrawing.Save();

        if (!worksheetPart.Worksheet.Elements<Drawing>().Any())
        {
            var drawingRelId = worksheetPart.GetIdOfPart(drawingsPart);
            worksheetPart.Worksheet.Append(new Drawing() { Id = drawingRelId });
        }

        worksheetPart.Worksheet.Save();
    }

    private static void ConfigurarCabecalho(IXLWorksheet sheet)
    {
        sheet.Row(1).Height = 20;
        sheet.Row(2).Height = 20;
        sheet.Row(3).Height = 20;
    }

    private static int EscreverInformacoesCabecalhoRelatorio(IXLWorksheet sheet, EscritaEfTurmaSondagemCabecalhoExcelDto dto)
    {
        int linha = 4;

        EscreverCelula(sheet, linha, 1, $"Ano letivo: {dto.AnoLetivo}");
        sheet.Range(linha, 1, linha, 3).Merge();

        EscreverCelula(sheet, linha, 4, $"Modalidade: {dto.Modalidade}");
        sheet.Range(linha, 4, linha, 6).Merge();

        EscreverCelula(sheet, linha, 7, $"Dre: {dto.Dre}");
        sheet.Range(linha, 7, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Unidade Educacional: {dto.Ue}");
        sheet.Range(linha, 1, linha, 6).Merge();

        EscreverCelula(sheet, linha, 7, $"Turma: {dto.Turma}");
        sheet.Range(linha, 7, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Proficiência: {dto.Proeficiencia}");
        sheet.Range(linha, 1, linha, 5).Merge();

        EscreverCelula(sheet, linha, 6, $"Bimestre: {dto.Semestre}");
        sheet.Range(linha, 6, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;

        EscreverCelula(sheet, linha, 1, $"Usuário: {dto.NomeUsuarioSolicitacao}");
        sheet.Range(linha, 1, linha, 5).Merge();

        EscreverCelula(sheet, linha, 6, $"Data de impressão: {dto.DataImpressao}");
        sheet.Range(linha, 6, linha, 10).Merge();

        AplicarBordaExterna(sheet.Range(linha, 1, linha, 10));
        linha++;
        linha++;

        return linha;
    }
}