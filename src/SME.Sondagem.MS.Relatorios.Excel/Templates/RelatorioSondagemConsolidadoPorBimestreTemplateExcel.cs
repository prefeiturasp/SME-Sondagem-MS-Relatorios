using ClosedXML.Excel;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

public class RelatorioSondagemConsolidadoPorBimestreTemplateExcel : RelatorioConsolidadoTemplateBase, IRelatorioSondagemConsolidadoPorBimestreTemplateExcel
{
    private const int ColsPerBlock = 6;
    private const int ColGap = 1;
    private const int BlockStep = ColsPerBlock + ColGap;

    private static readonly (string Header, string Key)[] ColunasFixasProficiencia3 =
    [
        ("Sondagem inicial", "Inicial"),
        ("1º bimestre",  "1° bimestre"),
        ("2º bimestre",  "2° bimestre"),
        ("3º bimestre",  "3° bimestre"),
        ("4º bimestre",  "4° bimestre"),
    ];

    public RelatorioSondagemConsolidadoPorBimestreTemplateExcel(IServicoArmazenamentoMinio servicoArmazenamentoMinio)
        : base(servicoArmazenamentoMinio) { }

    public async Task<string> GerarExcelEF(RelatorioConsolidadoSondagemDto dto)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Sondagem");

        EscreverCabecalhoConsolidado(sheet, dto);

        int ultimaLinha;
        if (dto.ProficienciaId == 3)
        {
            ultimaLinha = EscreverDadosProficiencia3(sheet, 8, dto);
        }
        else
        {
            var bimestres = ObterBimestres(dto);
            int linha = EscreverCabecalhoTabela(sheet, 7, bimestres);
            ultimaLinha = EscreverDadosExcel(sheet, linha, dto, bimestres);
        }

        const int dataColStart = 100;
        var graficos = ObterGraficosDasQuestoes(dto);
        EscreverDadosGraficos(sheet, graficos, ultimaLinha + 2, dataColStart);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        InjetarGraficosOpenXml(stream, graficos, ultimaLinha + 2, "Sondagem", ultimaLinha + 2, dataColStart);
        stream.Position = 0;

        return await EnviarExcelParaMinio(stream, dto.CodigoCorrelacao);
    }


    private static List<string> ObterBimestres(RelatorioConsolidadoSondagemDto dto)
    {
        return dto.Questoes
            .SelectMany(q => q.TotaisPorBimestre ?? [])
            .Select(b => b.Bimestre)
            .Distinct()
            .OrderBy(b => b)
            .ToList();
    }

    private static void EstilarCelulaDadosConsolidado(IXLCell cell, bool negrito = false)
    {
        EstilarCelulaDados(cell);
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = XLColor.DarkGray;
        if (negrito) cell.Style.Font.Bold = true;
    }

    private static int EscreverCabecalhoTabela(IXLWorksheet sheet, int linha, List<string> bimestres)
    {
        sheet.Column(1).Width = 40;
        EstilizarHeader(sheet.Cell(linha, 1), "Questão / Resposta");

        int col = 2;
        foreach (var bimestre in bimestres)
        {
            sheet.Column(col).Width = 20;
            EstilizarHeader(sheet.Cell(linha, col), bimestre);
            col++;
        }

        sheet.Row(linha).Height = 40;
        return linha + 1;
    }

    private static int EscreverDadosExcel(IXLWorksheet sheet, int linha, RelatorioConsolidadoSondagemDto dto, List<string> bimestres)
    {
        int totalColunas = 1 + bimestres.Count;

        foreach (var questao in dto.Questoes)
        {
            var celulaQuestao = sheet.Cell(linha, 1);
            celulaQuestao.Value = questao.QuestaoNome;
            celulaQuestao.Style.Font.Bold = true;
            celulaQuestao.Style.Fill.BackgroundColor = XLColor.LightGray;

            if (totalColunas > 1)
                sheet.Range(linha, 1, linha, totalColunas).Merge();

            AplicarBordaExterna(sheet.Range(linha, 1, linha, totalColunas));
            linha++;

            if (questao.Respostas == null) continue;

            foreach (var resposta in questao.Respostas)
            {
                var cor = !string.IsNullOrEmpty(resposta.CorFundo) ? ConverterCor(resposta.CorFundo) : XLColor.White;

                var celulaResposta = sheet.Cell(linha, 1);
                celulaResposta.Value = resposta.Resposta;
                celulaResposta.Style.Fill.BackgroundColor = cor;
                EstilarCelulaDadosConsolidado(celulaResposta, negrito: true);

                int col = 2;
                foreach (var bimestre in bimestres)
                {
                    var dadoBimestre = resposta.Bimestres?.FirstOrDefault(b => b.Bimestre == bimestre);
                    var cell = sheet.Cell(linha, col);
                    cell.Value = dadoBimestre != null ? $"{dadoBimestre.Quantidade} ({dadoBimestre.Percentual:F1}%)" : "-";
                    cell.Style.Fill.BackgroundColor = cor;
                    EstilarCelulaDadosConsolidado(cell);
                    col++;
                }

                linha++;
            }
        }

        return linha;
    }


    private static int EscreverDadosProficiencia3(IXLWorksheet sheet, int startRow, RelatorioConsolidadoSondagemDto dto)
    {
        var yearState = new Dictionary<string, (int StartRow, int NextCol)>(StringComparer.OrdinalIgnoreCase);
        int maxRowUsed = startRow;

        var corAlternada = XLColor.FromHtml("#F2F2F2");
        var corTotal     = XLColor.FromHtml("#D9D9D9");

        foreach (var questao in dto.Questoes)
        {
            var year = ExtrairAno(questao.QuestaoNome);

            if (!yearState.ContainsKey(year))
                yearState[year] = (StartRow: maxRowUsed, NextCol: 1);

            var (blocoRow, blocoCol) = yearState[year];
            int numRespostas = questao.Respostas?.Count() ?? 0;

            ConfigurarLarguraColunas(sheet, blocoCol);
            EscreverCabecalhoBloco(sheet, blocoRow, blocoCol, questao.QuestaoNome);
            EscreverDadosBloco(sheet, blocoRow + 1, blocoCol, questao, corAlternada);
            EscreverTotalBloco(sheet, blocoRow + 1 + numRespostas, blocoCol, questao, corTotal);

            yearState[year] = (blocoRow, blocoCol + BlockStep);

            // header + data rows + total + blank
            int blockHeight = 1 + numRespostas + 1 + 1;
            maxRowUsed = Math.Max(maxRowUsed, blocoRow + blockHeight);
        }

        return maxRowUsed;
    }

    private static string ExtrairAno(string questaoNome)
    {
        var inicio = questaoNome.LastIndexOf('(');
        var fim    = questaoNome.LastIndexOf(')');
        return inicio >= 0 && fim > inicio
            ? questaoNome.Substring(inicio + 1, fim - inicio - 1).Trim()
            : questaoNome;
    }

    private static void ConfigurarLarguraColunas(IXLWorksheet sheet, int colStart)
    {
        sheet.Column(colStart).Width = 30;
        for (int i = 1; i < ColsPerBlock; i++)
            sheet.Column(colStart + i).Width = 18;
    }

    private static void EscreverCabecalhoBloco(IXLWorksheet sheet, int linha, int colStart, string questaoNome)
    {
        var corHeader = XLColor.FromHtml("#F2F2F2");

        var celulaLabel = sheet.Cell(linha, colStart);
        celulaLabel.Value = questaoNome;
        celulaLabel.Style.Font.Bold = true;
        celulaLabel.Style.Fill.BackgroundColor = corHeader;
        celulaLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        celulaLabel.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        celulaLabel.Style.Alignment.WrapText    = true;
        celulaLabel.Style.Border.OutsideBorder  = XLBorderStyleValues.Thin;

        int col = colStart + 1;
        foreach (var (header, _) in ColunasFixasProficiencia3)
        {
            var cell = sheet.Cell(linha, col);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = corHeader;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText   = true;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            col++;
        }

        sheet.Row(linha).Height = 40;
    }

    private static void EscreverDadosBloco(IXLWorksheet sheet, int startLinha, int colStart, RelatorioConsolidadoQuestaoDto questao, XLColor corAlternada)
    {
        if (questao.Respostas == null) return;

        int idx   = 0;
        int linha = startLinha;

        foreach (var resposta in questao.Respostas)
        {
            var corFundo    = !string.IsNullOrEmpty(resposta.CorFundo) ? ConverterCor(resposta.CorFundo) : XLColor.White;
            var corTexto    = !string.IsNullOrEmpty(resposta.CorTexto) ? ConverterCor(resposta.CorTexto) : XLColor.Black;
            var corBimestre = idx % 2 == 0 ? XLColor.White : corAlternada;

            var celulaResposta = sheet.Cell(linha, colStart);
            celulaResposta.Value = resposta.Resposta;
            celulaResposta.Style.Fill.BackgroundColor = corFundo;
            celulaResposta.Style.Font.FontColor       = corTexto;
            EstilarCelulaDadosConsolidado(celulaResposta, negrito: true);

            int col = colStart + 1;
            foreach (var (_, key) in ColunasFixasProficiencia3)
            {
                var dado = resposta.Bimestres?.FirstOrDefault(b => b.Bimestre == key);
                var cell = sheet.Cell(linha, col);
                cell.Value = dado != null ? $"{dado.Quantidade} ({dado.Percentual:F1}%)" : "-";
                cell.Style.Fill.BackgroundColor = corBimestre;
                EstilarCelulaDadosConsolidado(cell);
                col++;
            }

            linha++;
            idx++;
        }
    }

    private static void EscreverTotalBloco(IXLWorksheet sheet, int linha, int colStart, RelatorioConsolidadoQuestaoDto questao, XLColor corTotal)
    {
        var celulaTotal = sheet.Cell(linha, colStart);
        celulaTotal.Value = "Total";
        celulaTotal.Style.Fill.BackgroundColor = corTotal;
        EstilarCelulaDadosConsolidado(celulaTotal, negrito: true);

        int col = colStart + 1;
        foreach (var (_, key) in ColunasFixasProficiencia3)
        {
            var total = questao.TotaisPorBimestre?.FirstOrDefault(b => b.Bimestre == key);
            var cell  = sheet.Cell(linha, col);
            cell.Value = total != null ? $"{total.Quantidade} ({total.Percentual:F1}%)" : "-";
            cell.Style.Fill.BackgroundColor = corTotal;
            EstilarCelulaDadosConsolidado(cell, negrito: true);
            col++;
        }
    }

    private static void EscreverDadosGraficos(IXLWorksheet sheet, List<(string Titulo, List<GraficoDto> Dados)> graficos, int dataRowBase, int dataColStart)
    {
        for (int i = 0; i < graficos.Count; i++)
        {
            var dados = graficos[i].Dados;
            int dataRow = dataRowBase + i * 12;
            for (int j = 0; j < dados.Count; j++)
            {
                sheet.Cell(dataRow, dataColStart + j).Value = dados[j].Descricao;
                sheet.Cell(dataRow + 1, dataColStart + j).Value = dados[j].Quantidade;
            }
        }
    }

    private static List<(string Titulo, List<GraficoDto> Dados)> ObterGraficosDasQuestoes(RelatorioConsolidadoSondagemDto dto)
    {
        return dto.Questoes
            .Select(q => (
                Titulo: q.QuestaoNome,
                Dados: (q.Respostas ?? [])
                    .Select(r => new GraficoDto
                    {
                        Descricao = r.Resposta,
                        Quantidade = r.Total,
                        Cor = r.CorFundo ?? "#CCCCCC"
                    })
                    .ToList()
            ))
            .ToList();
    }
}
