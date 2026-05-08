using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Globalization;
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

    public string GerarHtml(RelatorioConsolidadoSondagemDto dto)
    {
        var html = new StringBuilder();

        html.Append(GerarEstilos());
        html.Append("<body>");
        html.Append(GerarBlocosRelatorio(dto));
        html.Append("</body></html>");

        return html.ToString();
    }

    protected abstract string CssClasseColuna { get; }

    protected virtual string CssExtraSecaoTabelaH3 => string.Empty;

    protected virtual string GerarLinhaMetadadoDemografico(RelatorioConsolidadoSondagemDto dto) => string.Empty;

    protected abstract List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto);

    protected abstract HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao);

    protected abstract string FormatarCabecalhoColuna(string nomeColuna);

    protected abstract Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta);

    protected abstract Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao);

    protected abstract bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao);

    protected virtual bool ExibirLinhaFiltroDemografico => true;

    private string GerarBlocosRelatorio(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();

        if (dto.Questoes == null)
            return sb.ToString();

        var ordemColunas = ObterOrdemColunas(dto);

        foreach (var questao in dto.Questoes)
        {
            sb.Append("<div class=\"bloco-relatorio\">");
            sb.Append(GerarCabecalho(dto));
            sb.Append(GerarTabelaPorQuestao(questao, ordemColunas));
            sb.Append("</div>");
        }

        return sb.ToString();
    }

    protected virtual string GerarTabelaPorQuestao(RelatorioConsolidadoQuestaoDto questao, List<string> ordemColunas)
    {
        var colunas = ObterColunasOrdenadas(questao, ordemColunas);

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"secao-tabela\">");
        if (ExibirTituloQuestao)
            sb.AppendLine($"    <h3>{HttpUtility.HtmlEncode(questao.QuestaoNome)}</h3>");

        sb.AppendLine("    <table class=\"main-table\">");

        sb.AppendLine(GerarColgroup(colunas.Count));
        sb.AppendLine(GerarCabecalhoTabela(questao, colunas));
        sb.AppendLine(GerarCorpoTabela(questao, colunas));

        sb.AppendLine("    </table>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }

    protected virtual bool ExibirTituloQuestao => true;

    private List<string> ObterColunasOrdenadas(RelatorioConsolidadoQuestaoDto questao, List<string> ordemPreferida)
    {
        var encontradas = ColetarColunasEncontradas(questao);

        var ordenadas = ordemPreferida
            .Where(c => encontradas.Contains(c))
            .ToList();

        foreach (var coluna in encontradas.OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
        {
            if (!ordenadas.Contains(coluna, StringComparer.OrdinalIgnoreCase))
                ordenadas.Add(coluna);
        }

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

    protected virtual string ObterRotuloColunaNivel(RelatorioConsolidadoQuestaoDto questao) => "Nível";

    protected virtual string GerarCorpoTabela(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <tbody>");

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas.OrderBy(r => r.Ordem))
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
                    <td><strong>Ano:</strong> {HttpUtility.HtmlEncode(dto.AnoTurma)}</td>
                </tr>
                <tr>
                    <td><strong>Componente Curricular:</strong> {HttpUtility.HtmlEncode(dto.ComponenteCurricular)}</td>
                    <td colspan=""2""><strong>Proficiência:</strong> {HttpUtility.HtmlEncode(dto.Proficiencia)}</td>
                </tr>");

        if (ExibirLinhaFiltroDemografico)
        {
            sb.Append($@"
                <tr>
                    <td><strong>Bimestre:</strong> {HttpUtility.HtmlEncode(dto.Bimestre)}</td>
                    {GerarLinhaMetadadoDemografico(dto)}
                </tr>");
        }

        sb.Append($@"
                <tr>
                    <td colspan=""2""><strong>Usuário:</strong> {HttpUtility.HtmlEncode(dto.Usuario)}</td>
                    <td><strong>Data de impressão:</strong> {dto.DataImpressao:dd/MM/yyyy}</td>
                </tr>
            </table>");

        return sb.ToString();
    }

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
                            padding: 0 16px;
                            margin-top: 16px;
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

                        .badge-consolidacao {
                            display: inline-block;
                            background-color: #6933FF;
                            color: #fff;
                            font-size: 9px;
                            font-weight: 700;
                            padding: 4px 10px;
                            border-radius: 4px;
                            margin-top: 8px;
                            float: right;
                        }

                        .clearfix::after {
                            content: "";
                            display: table;
                            clear: both;
                        }

                        .bloco-relatorio + .bloco-relatorio {
                            page-break-before: always;
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
}
