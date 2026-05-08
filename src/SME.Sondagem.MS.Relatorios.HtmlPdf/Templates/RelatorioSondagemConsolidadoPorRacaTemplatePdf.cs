using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Globalization;
using System.Text;
using System.Web;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorRacaTemplatePdf : IRelatorioSondagemConsolidadoPorRacaTemplatePdf
{
    private static readonly CultureInfo PtBr = new("pt-BR");
    private static readonly string[] OrdemRacas =
    [
        "Preta",
        "Branca",
        "Parda",
        "Amarela",
        "Indígena",
        "Preferiu não informar",
        "Não informada"
    ];

    public string GerarHtml(RelatorioConsolidadoSondagemDto dto)
    {
        var html = new StringBuilder();

        html.Append(GerarEstilos());
        html.Append("<body>");
        html.Append(GerarTabelas(dto));
        html.Append("</body></html>");

        return html.ToString();
    }

    private static string GerarEstilos()
    {
        return """
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
                            margin: 12px 0 8px 0;
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
                        .col-raca { width: 12.5%; }

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

    private static string GerarCabecalho(RelatorioConsolidadoSondagemDto dto)
    {
        return $@"
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
                </tr>
                <tr>
                    <td><strong>Bimestre:</strong> {HttpUtility.HtmlEncode(dto.Bimestre)}</td>
                    <td colspan=""2""><strong>Raça:</strong> {HttpUtility.HtmlEncode(dto.Raca)}</td>
                </tr>
                <tr>
                    <td colspan=""2""><strong>Usuário:</strong> {HttpUtility.HtmlEncode(dto.Usuario)}</td>
                    <td><strong>Data de impressão:</strong> {dto.DataImpressao:dd/MM/yyyy}</td>
                </tr>
            </table>
        ";
    }

    private static string FormatarDataConsolidacao(DateTime? data)
    {
        return data.HasValue
            ? data.Value.ToString("dd/MM/yyyy 'às' HH:mm", PtBr)
            : "00/00/0000 às 00:00";
    }

    private static string GerarTabelas(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();

        if (dto.Questoes == null)
            return sb.ToString();

        foreach (var questao in dto.Questoes)
        {
            sb.Append("<div class=\"bloco-relatorio\">");
            sb.Append(GerarCabecalho(dto));
            sb.Append(GerarTabelaPorQuestao(questao));
            sb.Append("</div>");
        }

        return sb.ToString();
    }

    private static string GerarTabelaPorQuestao(RelatorioConsolidadoQuestaoDto questao)
    {
        var sb = new StringBuilder();
        var racas = ObterRacasOrdenadas(questao);

        sb.AppendLine("<div class=\"secao-tabela\">");
        sb.AppendLine($"    <h3>{HttpUtility.HtmlEncode(questao.QuestaoNome)}</h3>");
        sb.AppendLine("    <table class=\"main-table\">");

        sb.AppendLine(GerarColgroup(racas.Count));
        sb.AppendLine(GerarCabecalhoTabela(racas));
        sb.AppendLine(GerarCorpoTabela(questao, racas));

        sb.AppendLine("    </table>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }

    private static List<string> ObterRacasOrdenadas(RelatorioConsolidadoQuestaoDto questao)
    {
        var racasEncontradas = ColetarRacasEncontradas(questao);
        var ordenadas = OrdemRacas.Where(r => racasEncontradas.Contains(r)).ToList();
        ordenadas.AddRange(racasEncontradas.Where(r => !ordenadas.Contains(r, StringComparer.OrdinalIgnoreCase)));
        return ordenadas;
    }

    private static HashSet<string> ColetarRacasEncontradas(RelatorioConsolidadoQuestaoDto questao)
    {
        var racas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AdicionarRacasDasRespostas(racas, questao.Respostas);
        AdicionarRacasDosTotais(racas, questao.TotaisPorRaca);
        return racas;
    }

    private static void AdicionarRacasDasRespostas(HashSet<string> racas, IEnumerable<RelatorioConsolidadoRespostaDto>? respostas)
    {
        var validas = (respostas ?? [])
            .SelectMany(r => r.Racas ?? [])
            .Select(r => r.Raca)
            .Where(r => !string.IsNullOrWhiteSpace(r));

        foreach (var raca in validas)
            racas.Add(raca!);
    }

    private static void AdicionarRacasDosTotais(HashSet<string> racas, IEnumerable<RelatorioConsolidadoRacaDto>? totais)
    {
        var validas = (totais ?? [])
            .Select(raca => raca.Raca)
            .Where(r => !string.IsNullOrWhiteSpace(r));

        foreach (var raca in validas)
            racas.Add(raca!);
    }

    private static string GerarColgroup(int totalRacas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <colgroup>");
        sb.AppendLine("            <col class=\"col-nivel\" />");
        for (int i = 0; i < totalRacas; i++)
            sb.AppendLine("            <col class=\"col-raca\" />");
        sb.AppendLine("        </colgroup>");
        return sb.ToString();
    }

    private static string GerarCabecalhoTabela(List<string> racas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <thead>");
        sb.AppendLine("            <tr>");
        sb.AppendLine("                <th>Nível</th>");

        foreach (var raca in racas)
            sb.AppendLine($"                <th>{HttpUtility.HtmlEncode(raca)}</th>");

        sb.AppendLine("            </tr>");
        sb.AppendLine("        </thead>");
        return sb.ToString();
    }

    private static string GerarCorpoTabela(RelatorioConsolidadoQuestaoDto questao, List<string> racas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <tbody>");

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas.OrderBy(r => r.Ordem))
            {
                sb.AppendLine(GerarLinhaResposta(resposta, racas));
            }
        }

        if (questao.TotaisPorRaca != null && questao.TotaisPorRaca.Any())
        {
            sb.AppendLine(GerarLinhaTotal(questao, racas));
        }

        sb.AppendLine("        </tbody>");
        return sb.ToString();
    }

    private static string GerarLinhaResposta(RelatorioConsolidadoRespostaDto resposta, List<string> racas)
    {
        var sb = new StringBuilder();
        bool isSemPreenchimento = resposta.Resposta?.Trim()
            .Equals("Sem preenchimento", StringComparison.OrdinalIgnoreCase) == true;

        var classeLinha = isSemPreenchimento ? "row-sem-preenchimento" : string.Empty;
        sb.AppendLine($"            <tr class=\"{classeLinha}\">");

        sb.AppendLine(GerarBadgeNivel(resposta));

        var dicRacas = (resposta.Racas ?? [])
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var raca in racas)
        {
            if (dicRacas.TryGetValue(raca, out var valor))
                sb.AppendLine($"                <td>{GerarCelulaValor(valor.Quantidade, valor.Percentual)}</td>");
            else
                sb.AppendLine("                <td><span class=\"cell-vazio\">Vazio</span></td>");
        }

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    private static string GerarLinhaTotal(RelatorioConsolidadoQuestaoDto questao, List<string> racas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            <tr class=\"row-total\">");
        sb.AppendLine("                <td>Total</td>");

        var dicRacas = (questao.TotaisPorRaca ?? [])
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var raca in racas)
        {
            if (dicRacas.TryGetValue(raca, out var valor))
                sb.AppendLine($"                <td>{GerarCelulaValor(valor.Quantidade, valor.Percentual)}</td>");
            else
                sb.AppendLine("                <td><span class=\"cell-vazio\">-</span></td>");
        }

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    /// <summary>Gera a célula da coluna Nível: com cor, o fundo preenche toda a &lt;td&gt; (como no layout oficial).</summary>
    private static string GerarBadgeNivel(RelatorioConsolidadoRespostaDto resposta)
    {
        var texto = HttpUtility.HtmlEncode(resposta.Resposta ?? string.Empty);

        if (string.IsNullOrWhiteSpace(resposta.CorFundo))
            return $"                <td class=\"cell-nivel-texto\">{texto}</td>";

        var corFundo = resposta.CorFundo.Trim();
        var corTexto = string.IsNullOrWhiteSpace(resposta.CorTexto) ? "#ffffff" : resposta.CorTexto.Trim();

        return $"                <td class=\"cell-nivel-badge\" style=\"background-color: {corFundo}; color: {corTexto};\">{texto}</td>";
    }

    private static string GerarCelulaValor(int quantidade, double percentual)
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
}
