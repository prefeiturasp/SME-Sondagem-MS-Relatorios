using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Helpers;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

/// <summary>
/// Classe base para templates PDF de relatórios consolidados demográficos (raça, gênero, etc.).
/// Centraliza: estrutura HTML, estilos comuns, cabeçalho, badge de nível e formatação de células.
/// As subclasses fornecem apenas o que é específico de cada agrupamento demográfico.
/// </summary>
public abstract class RelatorioSondagemConsolidadoDemograficoTemplatePdfBase
{
    protected static readonly CultureInfo PtBr = new("pt-BR");
    private static readonly string FechaDiv = "</div>";
    private const string Todos = "Todos";

    public string GerarHtml(RelatorioConsolidadoSondagemDto dto)
    {
        var html = new StringBuilder();

        html.Append(GerarEstilos());
        html.Append("<body>");
        html.Append(GerarBlocosRelatorio(dto));
        html.Append("</body></html>");

        return html.ToString();
    }

    /// <summary>
    /// HTML completo (logo + meta de filtros) para o HeaderSettings.HtmlUrl do wkhtmltopdf
    /// (repetido no topo de cada página do PDF).
    /// </summary>
    public virtual string GerarHtmlDocumentoCabecalhoWk(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine(@"<meta charset=""utf-8"" />");
        sb.AppendLine("<style>");
        sb.AppendLine("* { box-sizing: border-box; margin: 0; padding: 0; }");
        sb.AppendLine("""
            body {
                font-family: 'Roboto', 'DejaVu Sans', Arial, sans-serif;
                font-size: 10px;
                color: #42474A;
                background: #fff;
                padding: 4px 16px 8px 16px;
                margin: 0;
                letter-spacing: 0.02em;
                word-spacing: 0.05em;
            }
            .header-logo { margin-bottom: 6px; }
            .header-logo img { height: 36px; }
            .report-title {
                text-align: center;
                margin-top: 8px;
                margin-bottom: 8px;
            }
            .report-title h1 {
                font-size: 14px;
                font-weight: 700;
                color: #42474A;
                margin-bottom: 2px;
                line-height: 1.2;
            }
            .report-title h2 {
                font-size: 10px;
                font-weight: 400;
                color: #42474A;
                line-height: 12px;
            }
            .meta-table {
                width: 100%;
                border-collapse: collapse;
                border: 1px solid #D9D9D9;
            }
            .meta-table td {
                border: 1px solid #D9D9D9;
                padding: 4px 8px;
                font-size: 10px;
                font-weight: 400;
                line-height: 12px;
                vertical-align: middle;
                color: #42474A;
            }
            .meta-table td strong {
                font-weight: 700;
                margin-right: 2px;
            }
            """);
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.Append(GerarCabecalho(dto));
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    protected abstract string CssClasseColuna { get; }

    protected virtual string CssExtraSecaoTabelaH3 => string.Empty;

    protected abstract List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto);

    protected abstract HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao);

    protected abstract string FormatarCabecalhoColuna(string nomeColuna);

    protected abstract Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta);

    protected abstract Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao);

    protected abstract bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao);

    protected virtual bool ExibirLinhaFiltroDemografico => true;

    protected virtual string GerarBlocosRelatorio(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();

        if (dto.Questoes == null)
            return sb.ToString();

        var ordemColunas = ObterOrdemColunas(dto);

        foreach (var questao in dto.Questoes)
        {
            sb.Append("<div class=\"bloco-relatorio\">");
            sb.Append(GerarTabelaPorQuestao(questao, ordemColunas));
            sb.AppendLine(FechaDiv);
        }

        foreach (var questao in dto.Questoes)
        {
            var grafico = MontarGraficoConsolidado(questao);
            var htmlGrafico = GerarGrafico(grafico, quebrarPaginaAntes: false);
            if (string.IsNullOrEmpty(htmlGrafico))
                continue;

            sb.Append("<div class=\"bloco-relatorio\">");
            sb.Append(htmlGrafico);
            sb.AppendLine(FechaDiv);
        }

        return sb.ToString();
    }

    protected virtual string GerarTabelaPorQuestao(RelatorioConsolidadoQuestaoDto questao, List<string> ordemColunas)
    {
        var colunas = ObterColunasOrdenadas(questao, ordemColunas);

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"secao-tabela\">");

        sb.AppendLine("    <table class=\"main-table\">");

        sb.AppendLine(GerarColgroup(colunas.Count));
        sb.AppendLine(GerarCabecalhoTabela(questao, colunas));
        sb.AppendLine(GerarCorpoTabela(questao, colunas));

        sb.AppendLine("    </table>");
        sb.AppendLine(FechaDiv);

        return sb.ToString();
    }

    protected virtual GraficoSondagemDto? MontarGraficoConsolidado(RelatorioConsolidadoQuestaoDto questao)
    {
        if (questao.Respostas == null)
            return null;

        var barras = questao.Respostas
            .Where(r => r.Total > 0)
            .OrderBy(r => r.Ordem)
            .Select(r => new GraficoBarraDto
            {
                Legenda = string.IsNullOrWhiteSpace(r.Resposta) ? "—" : r.Resposta,
                CorFundo = string.IsNullOrWhiteSpace(r.CorFundo) ? "#BFBFC2" : r.CorFundo,
                CorTexto = string.IsNullOrWhiteSpace(r.CorTexto) ? "#ffffff" : r.CorTexto,
                Quantidade = r.Total
            })
            .ToList();

        return barras.Count == 0
            ? null
            : new GraficoSondagemDto
            {
                Titulo = "Gráfico da Sondagem",
                Subtitulo = questao.QuestaoNome,
                Barras = barras
            };
    }

    protected virtual bool ExibirTituloQuestao => true;

    private List<string> ObterColunasOrdenadas(RelatorioConsolidadoQuestaoDto questao, List<string> ordemPreferida)
    {
        var encontradas = ColetarColunasEncontradas(questao);

        var ordenadas = ordemPreferida
            .Where(c => encontradas.Contains(c))
            .ToList();
        ordenadas.AddRange(from coluna in encontradas.OrderBy(c => c)
                           where !ordenadas.Contains(coluna, StringComparer.OrdinalIgnoreCase)
                           select coluna);
        return ordenadas;
    }

    protected virtual string GerarColgroup(int totalColunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <colgroup>");
        sb.AppendLine("            <col class=\"col-nivel\" />");
        for (int i = 0; i < totalColunas; i++)
            sb.AppendLine($"            <col class=\"{CssClasseColuna}\" />");
        sb.AppendLine("        </colgroup>");
        return sb.ToString();
    }

    protected virtual string GerarCabecalhoTabela(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <thead>");
        sb.AppendLine("            <tr>");
        sb.AppendLine($"                <th>{HttpUtility.HtmlEncode(ObterRotuloColunaNivel(questao))}</th>");

        foreach (var coluna in colunas)
            sb.AppendLine($"                <th>{HttpUtility.HtmlEncode(FormatarCabecalhoColuna(coluna))}</th>");

        sb.AppendLine("            </tr>");
        sb.AppendLine("        </thead>");
        return sb.ToString();
    }

    protected virtual string ObterRotuloColunaNivel(RelatorioConsolidadoQuestaoDto questao) => questao.QuestaoNome;

    protected virtual string GerarCorpoTabela(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <tbody>");

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas
                .Where(r => r != null)
                .OrderBy(r => r.Ordem))
                sb.AppendLine(GerarLinhaResposta(resposta, colunas));
        }

        if (PossuiTotais(questao))
            sb.AppendLine(GerarLinhaTotal(questao, colunas));

        sb.AppendLine("        </tbody>");
        return sb.ToString();
    }

    protected virtual string GerarLinhaResposta(RelatorioConsolidadoRespostaDto resposta, List<string> colunas)
    {
        var sb = new StringBuilder();
        bool isSemPreenchimento = resposta.Resposta?.Trim()
            .Equals("Sem preenchimento", StringComparison.OrdinalIgnoreCase) == true;

        var classeLinha = isSemPreenchimento ? "row-sem-preenchimento" : string.Empty;
        sb.AppendLine($"            <tr class=\"{classeLinha}\">");
        sb.AppendLine(GerarBadgeNivel(resposta));

        var dic = MontarDicionarioResposta(resposta);
        AppendCelulasValor(sb, colunas, dic);

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    protected virtual string GerarLinhaTotal(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            <tr class=\"row-total\">");
        sb.AppendLine("                <td>Total</td>");

        var dic = MontarDicionarioTotais(questao);
        AppendCelulasValor(sb, colunas, dic, vazioTexto: "-");

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    protected virtual void AppendCelulasValor(
        StringBuilder sb,
        List<string> colunas,
        Dictionary<string, (int Quantidade, double Percentual)> dic,
        string vazioTexto = "Vazio")
    {
        foreach (var coluna in colunas)
        {
            if (dic.TryGetValue(coluna, out var valor))
                sb.AppendLine($"                <td>{FormatarCelulaValor(valor.Quantidade, valor.Percentual)}</td>");
            else
                sb.AppendLine($"                <td><span class=\"cell-vazio\">{vazioTexto}</span></td>");
        }
    }

    protected static string FormatarCelulaValor(int quantidade, double percentual)
    {
        if (quantidade <= 0)
            return "<span class=\"cell-vazio\">Vazio</span>";

        var quantidadeFmt = quantidade.ToString("N0", PtBr);
        var percentualFmt = percentual.ToString("N2", PtBr) + "%";

        return $"""
                <div class="cell-valor">{quantidadeFmt}</div>
                <div class="cell-percentual">{percentualFmt}</div>
                """;
    }

    protected static string GerarBadgeNivel(RelatorioConsolidadoRespostaDto resposta)
    {
        var texto = HttpUtility.HtmlEncode(resposta.Resposta ?? string.Empty);

        if (string.IsNullOrWhiteSpace(resposta.CorFundo))
            return $"                <td class=\"cell-nivel-texto\">{texto}</td>";

        var corFundo = resposta.CorFundo.Trim();
        var corTexto = string.IsNullOrWhiteSpace(resposta.CorTexto) ? "#ffffff" : resposta.CorTexto.Trim();

        return $"                <td class=\"cell-nivel-badge\" style=\"background-color: {corFundo}; color: {corTexto};\">{texto}</td>";
    }

    protected virtual string GerarCabecalho(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();
        var (anoLabel, anoValue) = ObterLabelValorAno(dto);

        sb.Append($@"
            <div class=""header-logo"">
                <img src=""{SmeConstants.Logo_PrefSP_Horizontal}"" alt=""Prefeitura de São Paulo"" />
            </div>
            <div class=""report-title"">
                <h1>Relatório da Sondagem</h1>
                <h2>{HttpUtility.HtmlEncode(dto.Titulo)}</h2>
            </div>
            <table class=""meta-table"">
                <tr>
                    <td><strong>Agrupamento:</strong> {HttpUtility.HtmlEncode(dto.Agrupamento)}</td>
                    <td><strong>Ano letivo:</strong> {dto.AnoLetivo}</td>
                    <td><strong>Modalidade:</strong> {HttpUtility.HtmlEncode(dto.Modalidade)}</td>
                </tr>
                <tr>
                    <td><strong>DRE:</strong> {HttpUtility.HtmlEncode(dto.Dre)}</td>
                    <td><strong>Unidade Educacional:</strong> {HttpUtility.HtmlEncode(dto.UnidadeEducacional)}</td>
                    <td><strong>{anoLabel}:</strong> {HttpUtility.HtmlEncode(anoValue)}</td>
                </tr>
                <tr>
                    <td><strong>Componente Curricular:</strong> {HttpUtility.HtmlEncode(dto.ComponenteCurricular)}</td>
                    <td><strong>Proficiência:</strong> {HttpUtility.HtmlEncode(dto.Proficiencia)}</td>
                    <td><strong>Bimestre:</strong> {HttpUtility.HtmlEncode(dto.Bimestre)}</td>
                </tr>");

        AppendLinhasMetaFiltrosOpcionais(sb, dto);

        sb.Append($@"
                <tr>
                    <td colspan=""2""><strong>Usuário:</strong> {HttpUtility.HtmlEncode(dto.Usuario)}</td>
                    <td><strong>Data de impressão:</strong> {DataImpressaoBrasiliaFormatador.FormatarDataImpressaoFusoBrasilia(dto.DataImpressao)}</td>
                </tr>
            </table>");

        return sb.ToString();
    }

    private static (string Label, string Value) ObterLabelValorAno(RelatorioConsolidadoSondagemDto dto) =>
        dto.ModalidadeId switch
        {
            (int)Modalidade.EJA => ("Semestre: ", FormatarSemestre(dto.SemestreId)),
            (int)Modalidade.Fundamental => ("Ano: ", string.IsNullOrWhiteSpace(dto.AnoTurma) ? Todos : dto.AnoTurma),
            _ => ("Ano / Turma: ", string.IsNullOrWhiteSpace(dto.AnoTurma) ? Todos : dto.AnoTurma)
        };

    private static string FormatarSemestre(int semestreId)
    {
        if (semestreId == 0) return Todos;
        return Enum.TryParse(semestreId.ToString(), out Semestre s) ? s.ShortName() ?? Todos : Todos;
    }

    internal static void AppendLinhasMetaFiltrosOpcionais(StringBuilder sb, RelatorioConsolidadoSondagemDto dto)
    {
        var celulas = MontarCelulasMetaFiltrosOpcionais(dto);
        if (celulas.Count == 0)
            return;

        AppendLinhasMetaTabelaEmBlocos(sb, celulas, maxCelulasPorLinha: 3);
    }

    private static List<string> MontarCelulasMetaFiltrosOpcionais(RelatorioConsolidadoSondagemDto dto)
    {
        var celulas = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.Genero))
            celulas.Add(MetaFiltroCelulaTexto("Gênero", dto.Genero.Trim()));

        if (!string.IsNullOrWhiteSpace(dto.Raca))
            celulas.Add(MetaFiltroCelulaTexto("Raça", dto.Raca.Trim()));

        var programas = ColetarRotulosProgramasEAtendimentosAtivos(dto);
        if (programas.Count > 0)
            celulas.Add(MetaFiltroCelulaTexto("Programas e Atendimentos", string.Join(", ", programas)));

        if (dto.PossuiLinguaPortuguesaSegundaLingua.HasValue)
            celulas.Add(MetaFiltroCelulaBool("Português como segunda língua", dto.PossuiLinguaPortuguesaSegundaLingua.Value));

        return celulas;
    }

    private static List<string> ColetarRotulosProgramasEAtendimentosAtivos(RelatorioConsolidadoSondagemDto dto)
    {
        return [.. (from par in new (bool? Ativo, string Rotulo)[]
                 {
                     (dto.Pap, "PAP"),
                     (dto.Aee, "AEE"),
                     (dto.Deficiente, "Deficiente")
                 }
                where par.Ativo is true
                select par.Rotulo)];
    }

    private static void AppendLinhasMetaTabelaEmBlocos(StringBuilder sb, List<string> celulas, int maxCelulasPorLinha)
    {
        foreach (var bloco in celulas.Chunk(maxCelulasPorLinha))
        {
            sb.AppendLine("                <tr>");
            foreach (var cel in bloco)
                sb.AppendLine($"                    {cel}");
            for (var k = bloco.Length; k < maxCelulasPorLinha; k++)
                sb.AppendLine("                    <td></td>");
            sb.AppendLine("                </tr>");
        }
    }

    private static string MetaFiltroCelulaBool(string rotulo, bool valor) =>
        $"<td><strong>{HttpUtility.HtmlEncode(rotulo)}:</strong> {(valor ? "Sim" : "Não")}</td>";

    private static string MetaFiltroCelulaTexto(string rotulo, string valor) =>
        $"<td><strong>{HttpUtility.HtmlEncode(rotulo)}:</strong> {HttpUtility.HtmlEncode(valor)}</td>";

    private const string FechaDivConsolidado = "</div>";

    private string GerarEstilos()
    {
        var cssExtraH3 = string.IsNullOrEmpty(CssExtraSecaoTabelaH3) ? "" : $"\n                        {CssExtraSecaoTabelaH3}";

        return $$"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8" />
                    <style>
                        * { box-sizing: border-box; margin: 0; padding: 0; }
                        body {
                            font-family: 'Roboto', 'DejaVu Sans', Arial, sans-serif;
                            font-size: 10px;
                            color: #42474A;
                            background: #fff;
                            padding: 12px 16px 0 16px;
                            letter-spacing: 0.02em;
                            word-spacing: 0.05em;
                        }

                        /* Fluxo contínuo: sem page-break-before entre blocos (empilha se couber). */
                        .bloco-relatorio {
                            page-break-inside: avoid;
                        }
                        .secao-tabela {
                            margin-top: 16px;
                            page-break-inside: avoid;
                        }
                        .secao-tabela h3 {
                            font-size: 11px;
                            font-weight: 700;
                            color: #42474A;
                            text-align: center;
                            margin: 12px 0 8px 0;{{cssExtraH3}}
                        }

                        .main-table {
                            width: 100%;
                            border-collapse: collapse;
                            table-layout: fixed;
                        }
                        .main-table thead th {
                            border: 1px solid #F0F0F0;
                            padding: 8px;
                            text-align: center;
                            vertical-align: middle;
                            font-size: 10px;
                            font-weight: 700;
                            line-height: 12px;
                            background-color: #FAFAFA;
                            color: #42474A;
                        }
                        .main-table td {
                            border: 1px solid #F0F0F0;
                            padding: 6px 4px;
                            text-align: center;
                            vertical-align: middle;
                            font-size: 10px;
                            line-height: 12px;
                            color: #42474A;
                            background-color: #fff;
                        }

                        .col-nivel { width: 12%; }
                        .{{CssClasseColuna}} { width: 12.5%; }

                        .main-table td.cell-nivel-badge {
                            border: 2px solid #FFFFFF;
                            font-weight: 700;
                            font-size: 10px;
                            line-height: 14px;
                            border-radius: 10px;
                            vertical-align: middle;
                            text-align: center;
                            background-clip: padding-box;
                        }

                        .main-table td.cell-nivel-texto {
                            padding: 16px 12px;
                            font-weight: 400;
                            line-height: 16px;
                            vertical-align: middle;
                            box-sizing: border-box;
                        }

                        .cell-valor { font-weight: 700; }
                        .cell-percentual {
                            font-weight: 400;
                            color: #6c757d;
                            font-size: 9px;
                        }
                        .cell-vazio {
                            color: #BFBFC2;
                            font-weight: 500;
                        }

                        .row-total td {
                            font-weight: 700;
                            background-color: #FAFAFA;
                        }
                        .row-sem-preenchimento td {
                            background-color: #FFFFFF;
                        }
                    </style>
                </head>
                """;
    }

    public static string GerarGrafico(GraficoSondagemDto? model, bool quebrarPaginaAntes = true)
    {
        if (model?.Barras == null || model.Barras.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var ptBr = PtBr;

        var barras = model.Barras;
        int maxValor = barras.Max(b => b.Quantidade);
        if (maxValor == 0) maxValor = 1;

        int stepCount = 7;
        double rawStep = (double)maxValor / (stepCount - 1);
        double rawStepParaLog = rawStep > 0 ? rawStep : 1d;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStepParaLog)));
        double niceStep = Math.Ceiling(rawStep / magnitude) * magnitude;

        // niceStep pode ser fracionário (ex.: máx. 3 → 0,5); (int) truncaria para 0 e todos os rótulos do eixo Y virariam "0".
        int yStep = Math.Max(1, (int)Math.Ceiling(niceStep));
        int yMax = yStep * (stepCount - 1);

        int chartHeight = 260;
        int rowHeight = chartHeight / (stepCount - 1);
        int areaWidth = 694;
        int barMargin = 12;
        int barPadLeft = 8;
        int barPadRight = 8;
        int barWidth = (areaWidth - barPadLeft - barPadRight - (barras.Count * barMargin)) / barras.Count;
        if (barWidth < 1) barWidth = 70;

        string svgYLabel = "data:image/svg+xml;charset=utf-8,"
            + "%3Csvg%20xmlns%3D'http%3A//www.w3.org/2000/svg'%20width%3D'16'%20height%3D'"
            + chartHeight.ToString()
            + "'%3E%3Ctext%20x%3D'-"
            + (chartHeight / 2).ToString()
            + "'%20y%3D'12'%20transform%3D'rotate(-90)'%20font-family%3D'Arial%2Csans-serif'"
            + "%20font-size%3D'9'%20fill%3D'%2342474A'%20text-anchor%3D'middle'%3E"
            + "Quantidade%20de%20estudantes%3C/text%3E%3C/svg%3E";

        var aberturaBlocoTopo = quebrarPaginaAntes
            ? "<div style=\"page-break-before: always; padding-top: 20px; padding-bottom: 8px; margin-bottom: 32px;\">"
            : "<div style=\"padding-top: 24px; padding-bottom: 8px; margin-bottom: 32px;\">";
        sb.AppendLine(aberturaBlocoTopo);
        sb.AppendLine("    <div style=\"text-align:center; margin-bottom:24px;\">");
        sb.AppendLine($"        <h1 style=\"font-size:14px; font-weight:700; color:#42474A; margin-bottom:4px;\">{model.Titulo}</h1>");
        sb.AppendLine($"        <h2 style=\"font-size:10px; font-weight:400; color:#42474A;\">{model.Subtitulo}</h2>");
        sb.AppendLine("    </div>");
        sb.AppendLine("    <table style=\"width:auto; border-collapse:collapse; border:none;\">");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <td style=\"width:16px; padding:0; border:none; vertical-align:middle; text-align:center;\">");
        sb.AppendLine($"                <img src=\"{svgYLabel}\" style=\"width:16px; height:{chartHeight}px; display:block;\" />");
        sb.AppendLine("            </td>");
        sb.AppendLine("            <td style=\"width:38px; padding:0; border:none; vertical-align:top;\">");

        GraficoAppendYLabels(sb, stepCount, yStep, rowHeight, ptBr);

        sb.AppendLine("            </td>");
        sb.AppendLine($"            <td style=\"width:{areaWidth}px; padding:0; border:none; vertical-align:top;\">");

        GraficoAppendChartRows(sb, stepCount, rowHeight, areaWidth);

        sb.AppendLine($"                <div style=\"margin-top:-{chartHeight}px; white-space:nowrap; padding-left:{barPadLeft}px;\">");
        GraficoAppendBarDivs(sb, barras, yMax, chartHeight, barWidth, barMargin);
        sb.AppendLine("                </div>");

        sb.AppendLine($"                <div style=\"white-space:nowrap; padding-left:{barPadLeft}px; margin-top:4px;\">");
        GraficoAppendBarLegendas(sb, barras, barWidth, barMargin);
        sb.AppendLine("                </div>");

        sb.AppendLine("                <div style=\"text-align:center; font-size:9px; font-weight:700; color:#42474A; margin-top:6px;\">");
        sb.AppendLine("                    Opções de respostas");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </td>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </table>");
        sb.AppendLine(FechaDivConsolidado);

        return sb.ToString();
    }

    private static void GraficoAppendYLabels(StringBuilder sb, int stepCount, int yStep, int rowHeight, CultureInfo ptBr)
    {
        for (int i = stepCount - 1; i >= 0; i--)
        {
            var nivelValor = yStep * i;
            sb.AppendLine($"                <div style=\"height:{rowHeight}px; width:100%;\">");
            sb.AppendLine($"                    <table style=\"width:100%; height:{rowHeight}px; border-collapse:collapse;\">");
            sb.AppendLine("                        <tr>");
            sb.AppendLine($"                            <td style=\"vertical-align:bottom; text-align:right; padding:0 4px 1px 0; font-size:8px; color:#42474A; white-space:nowrap; border:none;\">");
            sb.AppendLine($"                                {nivelValor.ToString("N0", ptBr)}");
            sb.AppendLine("                            </td>");
            sb.AppendLine("                        </tr>");
            sb.AppendLine("                    </table>");
            sb.AppendLine(FechaDivConsolidado);
        }
    }

    private static void GraficoAppendChartRows(StringBuilder sb, int stepCount, int rowHeight, int areaWidth)
    {
        for (int i = stepCount - 1; i >= 0; i--)
        {
            var border = (i == 0) ? "border-bottom:2px solid #9E9E9E;" : "border-bottom:1px solid #E0E0E0;";
            sb.AppendLine($"                <div style=\"height:{rowHeight}px; width:{areaWidth}px; {border}\"></div>");
        }
    }

    private static void GraficoAppendBarDivs(StringBuilder sb, List<GraficoBarraDto> barras, int yMax, int chartHeight, int barWidth, int barMargin)
    {
        foreach (var barra in barras)
        {
            int altPx = yMax > 0 ? (int)((double)barra.Quantidade / yMax * chartHeight) : 0;
            if (barra.Quantidade > 0 && altPx < 15) altPx = 15;
            int paddingTop = chartHeight - altPx;

            sb.AppendLine($"                    <div style=\"display:inline-block; width:{barWidth}px; margin-right:{barMargin}px; vertical-align:top; padding-top:{paddingTop}px;\">");
            if (barra.Quantidade > 0)
            {
                sb.AppendLine($"                        <div style=\"height:{altPx}px; background-color:{barra.CorFundo}; border-radius:4px 4px 0 0; text-align:center; overflow:hidden;\">");
                sb.AppendLine($"                            <span style=\"display:block; font-size:8px; font-weight:700; color:{barra.CorTexto}; padding-top:3px; line-height:12px;\">");
                sb.AppendLine($"                                {barra.Quantidade}");
                sb.AppendLine("                            </span>");
                sb.AppendLine("                        </div>");
            }
            sb.AppendLine(FechaDivConsolidado);
        }
    }

    private static void GraficoAppendBarLegendas(StringBuilder sb, List<GraficoBarraDto> barras, int barWidth, int barMargin)
    {
        foreach (var barra in barras)
        {
            sb.AppendLine($"                    <div style=\"display:inline-block; width:{barWidth}px; margin-right:{barMargin}px; text-align:center; font-size:8px; color:#42474A; white-space:normal; vertical-align:top;\">");
            sb.AppendLine($"                        {barra.Legenda}");
            sb.AppendLine($"                    </div>");
        }
    }
}
