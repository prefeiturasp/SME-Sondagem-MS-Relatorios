using ClosedXML.Excel;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Excel.Templates;

public class RelatorioSondagemConsolidadoGenericoTemplateExcel
    : RelatorioConsolidadoTemplateBase,
      IRelatorioSondagemConsolidadoGenericoTemplateExcel
{
    private const int ColGap = 1;
    private const string FormatoValor = "{0} ({1:F1}%)";

    private static readonly Func<RelatorioConsolidadoRacaDto, int> OrdemRaca =
        r => r.Raca.Trim().Contains(' ') ? 1 : 0;

    private sealed record ColDefinition(string Header, string Key, string? Grupo = null);

    public RelatorioSondagemConsolidadoGenericoTemplateExcel(IServicoArmazenamentoMinio servicoArmazenamentoMinio)
        : base(servicoArmazenamentoMinio) { }

    public async Task<string> GerarExcelEF(RelatorioConsolidadoSondagemDto relatorioConsolidadoSondagemDto)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Sondagem");

        EscreverCabecalhoConsolidado(sheet, relatorioConsolidadoSondagemDto);

        var (ultimaLinha, questaoPositions) = EscreverDadosQuestoes(sheet, 8, relatorioConsolidadoSondagemDto);

        const int dataColStart = 100;
        var graficosBase = ObterGraficosDasQuestoes(relatorioConsolidadoSondagemDto);
        var graficos = graficosBase
            .Zip(questaoPositions, (g, pos) => (g.Titulo, g.Dados, pos.ColStart, pos.ColCount))
            .ToList();

        EscreverCabecalhoGraficos(sheet, graficos, ultimaLinha + 2);
        EscreverDadosGraficos(sheet, graficos, ultimaLinha + 2, dataColStart);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        InjetarGraficosOpenXml(stream, graficos, ultimaLinha + 2, "Sondagem", ultimaLinha + 2, dataColStart);
        stream.Position = 0;

        return await EnviarExcelParaMinio(stream, relatorioConsolidadoSondagemDto.CodigoCorrelacao);
    }

    private static List<ColDefinition> ExtrairColunas(RelatorioConsolidadoQuestaoDto questao)
    {
        var firstResposta = questao.Respostas?.FirstOrDefault();

        if (firstResposta?.GenerosComRacas?.Any() == true)
            return ExtrairColunasGenerosComRacas(questao, firstResposta);

        if (firstResposta?.Generos?.Any() == true)
        {
            var source = questao.TotaisPorGenero ?? firstResposta.Generos;
            return source
                .Select(g => new ColDefinition(g.Genero ?? g.Sigla ?? string.Empty, g.Genero ?? string.Empty))
                .ToList();
        }

        if (firstResposta?.Racas?.Any() == true)
        {
            var source = questao.TotaisPorRaca ?? firstResposta.Racas;
            return source
                .OrderBy(OrdemRaca)
                .Select(r => new ColDefinition(r.Raca, r.Raca))
                .ToList();
        }

        if (firstResposta?.Bimestres?.Any() == true)
        {
            var source = questao.TotaisPorBimestre ?? firstResposta.Bimestres;
            return source
                .Select(b => new ColDefinition(b.Bimestre, b.Bimestre))
                .ToList();
        }

        if (firstResposta?.AnosTurma?.Any() == true)
        {
            var source = questao.TotaisPorAnoTurma ?? firstResposta.AnosTurma;
            return source
                .OrderBy(a => a.AnoTurma)
                .Select(a => new ColDefinition(a.AnoTurma.ToString(), a.AnoTurma.ToString()))
                .ToList();
        }

        return [];
    }

    private static List<ColDefinition> ExtrairColunasGenerosComRacas(RelatorioConsolidadoQuestaoDto questao, RelatorioConsolidadoRespostaDto firstResposta)
    {
        var source = questao.TotaisPorGeneroComRacas ?? firstResposta.GenerosComRacas!;
        return source
            .SelectMany(genero => (genero.Racas ?? [])
                .OrderBy(OrdemRaca)
                .Select(r => new ColDefinition(r.Raca, $"{genero.Genero}|{r.Raca}", genero.Genero)))
            .ToList();
    }

    private static (int Quantidade, double Percentual)? GetValorCelula(RelatorioConsolidadoRespostaDto resposta, string key) =>
        ResolverValor(resposta.GenerosComRacas, resposta.Generos, resposta.Racas, resposta.Bimestres, resposta.AnosTurma, key);

    private static (int Quantidade, double Percentual)? GetValorTotal(RelatorioConsolidadoQuestaoDto questao, string key) =>
        ResolverValor(questao.TotaisPorGeneroComRacas, questao.TotaisPorGenero, questao.TotaisPorRaca, questao.TotaisPorBimestre, questao.TotaisPorAnoTurma, key);

    private static (int Quantidade, double Percentual)? ResolverValor(
        IEnumerable<RelatorioConsolidadoGeneroRacaDto>? generosComRacas,
        IEnumerable<RelatorioConsolidadoGeneroDto>? generos,
        IEnumerable<RelatorioConsolidadoRacaDto>? racas,
        IEnumerable<RelatorioConsolidadoBimestreDto>? bimestres,
        IEnumerable<RelatorioConsolidadoAnoTurmaDto>? anosturma,
        string key)
    {
        if (generosComRacas != null)
            return ResolverValorGeneroComRaca(generosComRacas, key);
        if (generos != null)
        {
            var g = generos.FirstOrDefault(g => g.Genero == key);
            return g != null ? (g.Quantidade, g.Percentual) : null;
        }
        if (racas != null)
        {
            var r = racas.FirstOrDefault(r => r.Raca == key);
            return r != null ? (r.Quantidade, r.Percentual) : null;
        }
        if (bimestres != null)
        {
            var b = bimestres.FirstOrDefault(b => b.Bimestre == key);
            return b != null ? (b.Quantidade, b.Percentual) : null;
        }
        if (anosturma != null)
        {
            var a = anosturma.FirstOrDefault(a => a.AnoTurma.ToString() == key);
            return a != null ? (a.Quantidade, a.Percentual) : null;
        }
        return null;
    }

    private static (int Quantidade, double Percentual)? ResolverValorGeneroComRaca(
        IEnumerable<RelatorioConsolidadoGeneroRacaDto> generosComRacas, string key)
    {
        var parts = key.Split('|');
        if (parts.Length != 2) return null;
        var genero = generosComRacas.FirstOrDefault(g => g.Genero == parts[0]);
        var r = genero?.Racas?.FirstOrDefault(r => r.Raca == parts[1]);
        return r != null ? (r.Quantidade, r.Percentual) : null;
    }

    private static void AplicarEstiloCelulaHeader(IXLCell cell, XLColor corHeader)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Fill.BackgroundColor = corHeader;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText   = true;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    private static void EscreverColunaValores(
        IXLWorksheet sheet, int linha, int colStart,
        List<ColDefinition> colunas,
        Func<ColDefinition, (int Quantidade, double Percentual)?> obterValor,
        XLColor corFundo, bool negrito = false)
    {
        int col = colStart + 1;
        foreach (var colDef in colunas)
        {
            var valor = obterValor(colDef);
            var cell  = sheet.Cell(linha, col);
            cell.Value = valor != null ? string.Format(FormatoValor, valor.Value.Quantidade, valor.Value.Percentual) : "Vazio";
            cell.Style.Fill.BackgroundColor = corFundo;
            EstilarCelulaDadosConsolidado(cell, negrito);
            if (valor == null) cell.Style.Font.FontColor = XLColor.Gray;
            col++;
        }
    }

    private static void EstilarCelulaDadosConsolidado(IXLCell cell, bool negrito = false)
    {
        EstilarCelulaDados(cell);
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = XLColor.DarkGray;
        if (negrito) cell.Style.Font.Bold = true;
    }

private static (int MaxRowUsed, List<(int ColStart, int ColCount)> QuestaoPositions) EscreverDadosQuestoes(IXLWorksheet sheet, int startRow, RelatorioConsolidadoSondagemDto dto)
    {
        var yearState = new Dictionary<string, (int StartRow, int NextCol)>(StringComparer.OrdinalIgnoreCase);
        int maxRowUsed = startRow;
        var questaoPositions = new List<(int ColStart, int ColCount)>();

        var corAlternada = XLColor.FromHtml("#F2F2F2");
        var corTotal     = XLColor.FromHtml("#D9D9D9");

        var todasColunas = dto.Questoes.Select(ExtrairColunas).ToList();
        int maxColunas = todasColunas.Count > 0 ? todasColunas.Max(c => c.Count) : 0;
        int blockStep = 1 + maxColunas + ColGap;

        int questaoIdx = 0;
        foreach (var questao in dto.Questoes)
        {
            var colunas = todasColunas[questaoIdx++];
            if (colunas.Count == 0)
            {
                questaoPositions.Add((1, 5));
                continue;
            }

            var year = ExtrairAno(questao.QuestaoNome);

            if (!yearState.ContainsKey(year))
                yearState[year] = (StartRow: maxRowUsed, NextCol: 1);

            var (blocoRow, blocoCol) = yearState[year];
            int numRespostas = questao.Respostas?.Count() ?? 0;

            ConfigurarLarguraColunas(sheet, blocoCol, colunas.Count);
            int headerRows = EscreverCabecalhoBloco(sheet, blocoRow, blocoCol, questao.QuestaoNome, colunas);
            EscreverDadosBloco(sheet, blocoRow + headerRows, blocoCol, questao, colunas, corAlternada);
            EscreverTotalBloco(sheet, blocoRow + headerRows + numRespostas, blocoCol, questao, colunas, corTotal);

            questaoPositions.Add((blocoCol, 1 + colunas.Count));
            yearState[year] = (blocoRow, blocoCol + blockStep);

            int blockHeight = headerRows + numRespostas + 1 + 1;
            maxRowUsed = Math.Max(maxRowUsed, blocoRow + blockHeight);
        }

        return (maxRowUsed, questaoPositions);
    }

    private static string ExtrairAno(string questaoNome)
    {
        var inicio = questaoNome.LastIndexOf('(');
        var fim    = questaoNome.LastIndexOf(')');
        return inicio >= 0 && fim > inicio
            ? questaoNome.Substring(inicio + 1, fim - inicio - 1).Trim()
            : questaoNome;
    }

    private static void ConfigurarLarguraColunas(IXLWorksheet sheet, int colStart, int numColunas)
    {
        sheet.Column(colStart).Width = 30;
        for (int i = 1; i <= numColunas; i++)
            sheet.Column(colStart + i).Width = 15;
    }

    private static int EscreverCabecalhoBloco(IXLWorksheet sheet, int linha, int colStart, string questaoNome, List<ColDefinition> colunas)
    {
        var corHeader = XLColor.FromHtml("#F2F2F2");
        bool hasGroups = colunas.Any(c => c.Grupo != null);
        int headerRows = hasGroups ? 2 : 1;

        var celulaLabel = sheet.Cell(linha, colStart);
        celulaLabel.Value = questaoNome;
        AplicarEstiloCelulaHeader(celulaLabel, corHeader);

        if (hasGroups)
        {
            sheet.Range(sheet.Cell(linha, colStart), sheet.Cell(linha + 1, colStart)).Merge();

            int col = colStart + 1;
            foreach (var grupo in colunas.GroupBy(c => c.Grupo))
            {
                int count = grupo.Count();
                var range = sheet.Range(sheet.Cell(linha, col), sheet.Cell(linha, col + count - 1));
                if (count > 1) range.Merge();
                var cell = sheet.Cell(linha, col);
                cell.Value = grupo.Key ?? string.Empty;
                AplicarEstiloCelulaHeader(cell, corHeader);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.OutsideBorderColor = XLColor.Black;
                col += count;
            }
            sheet.Row(linha).Height = 25;
            linha++;
        }

        int colIdx = colStart + 1;
        foreach (var colDef in colunas)
        {
            var cell = sheet.Cell(linha, colIdx);
            cell.Value = colDef.Header;
            AplicarEstiloCelulaHeader(cell, corHeader);
            colIdx++;
        }

        sheet.Row(linha).Height = 40;
        return headerRows;
    }

    private static void EscreverDadosBloco(IXLWorksheet sheet, int startLinha, int colStart, RelatorioConsolidadoQuestaoDto questao, List<ColDefinition> colunas, XLColor corAlternada)
    {
        int idx   = 0;
        int linha = startLinha;

        foreach (var resposta in questao.Respostas ?? [])
        {
            var corFundo = !string.IsNullOrEmpty(resposta.CorFundo) ? ConverterCor(resposta.CorFundo) : XLColor.White;
            var corTexto = !string.IsNullOrEmpty(resposta.CorTexto) ? ConverterCor(resposta.CorTexto) : XLColor.Black;
            var corLinha = idx % 2 == 0 ? XLColor.White : corAlternada;

            var celulaResposta = sheet.Cell(linha, colStart);
            celulaResposta.Value = resposta.Resposta;
            celulaResposta.Style.Fill.BackgroundColor = corFundo;
            celulaResposta.Style.Font.FontColor       = corTexto;
            EstilarCelulaDadosConsolidado(celulaResposta, negrito: true);

            EscreverColunaValores(sheet, linha, colStart, colunas, colDef => GetValorCelula(resposta, colDef.Key), corLinha);

            AjustarAlturaLinha(sheet, linha, resposta.Resposta);
            linha++;
            idx++;
        }
    }

    private static void EscreverTotalBloco(IXLWorksheet sheet, int linha, int colStart, RelatorioConsolidadoQuestaoDto questao, List<ColDefinition> colunas, XLColor corTotal)
    {
        var celulaTotal = sheet.Cell(linha, colStart);
        celulaTotal.Value = "Total";
        celulaTotal.Style.Fill.BackgroundColor = corTotal;
        EstilarCelulaDadosConsolidado(celulaTotal, negrito: true);

        EscreverColunaValores(sheet, linha, colStart, colunas, colDef => GetValorTotal(questao, colDef.Key), corTotal, negrito: true);
    }

    private static void EscreverDadosGraficos(IXLWorksheet sheet, List<(string Titulo, List<GraficoDto> Dados, int ColStart, int ColCount)> graficos, int dataRowBase, int dataColStart)
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

    private static void EscreverCabecalhoGraficos(IXLWorksheet sheet, List<(string Titulo, List<GraficoDto> Dados, int ColStart, int ColCount)> graficos, int linhaInicio)
    {
        for (int i = 0; i < graficos.Count; i++)
        {
            var (titulo, _, colStart, colCount) = graficos[i];
            int colEnd = colStart + colCount - 1;
            int linha = linhaInicio + i * 26;
            double totalWidth = 30.0 + (colCount - 1) * 15.0;

            if (colCount > 1) sheet.Range(linha, colStart, linha, colEnd).Merge();
            var cell1 = sheet.Cell(linha, colStart);
            cell1.Value = "Gráfico da Sondagem";
            cell1.Style.Font.Bold = true;
            cell1.Style.Font.FontSize = 16;
            cell1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AjustarAlturaLinha(sheet, linha, cell1.Value.ToString(), totalWidth);

            if (colCount > 1) sheet.Range(linha + 1, colStart, linha + 1, colEnd).Merge();
            var cell2 = sheet.Cell(linha + 1, colStart);
            var tituloSemAno = RemoverAno(titulo);
            cell2.Value = tituloSemAno;
            cell2.Style.Font.Bold = true;
            cell2.Style.Font.FontSize = 13;
            cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AjustarAlturaLinha(sheet, linha + 1, tituloSemAno, totalWidth);

            if (colCount > 1) sheet.Range(linha + 2, colStart, linha + 2, colEnd).Merge();
            var cell3 = sheet.Cell(linha + 2, colStart);
            const string descricao = "O gráfico apresenta o total de alunos em cada pergunta, sem aplicar recortes por raça, gênero ou raça e gênero.";
            cell3.Value = descricao;
            cell3.Style.Alignment.WrapText = true;
            cell3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            AjustarAlturaLinha(sheet, linha + 2, descricao, totalWidth);
        }
    }

    private static string RemoverAno(string questaoNome)
    {
        var inicio = questaoNome.LastIndexOf('(');
        return inicio > 0 ? questaoNome[..inicio].Trim() : questaoNome;
    }
}
