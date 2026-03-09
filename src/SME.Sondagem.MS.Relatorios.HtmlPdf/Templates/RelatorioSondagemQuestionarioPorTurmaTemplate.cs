using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Globalization;
using System.Text;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public static class RelatorioSondagemQuestionarioPorTurmaTemplate
{
    public static string GerarHtml(ConsultaSondagemPorTurmaDto dto)
    {
        var html = new StringBuilder();

        html.Append("""
                        <!DOCTYPE html>
                                    <html>
                                    <head>
                                        <meta charset="utf-8"/>
                                        <style>
                                        * { box-sizing: border-box; margin: 0; padding: 0; }
                                        body {
                                            font-family: 'Roboto', 'DejaVu Sans', Arial, sans-serif;
                                            font-size: 10px;
                                            color: #42474A;
                                            padding: 10px 16px 4px 16px;
                                            background: #fff;
                                            /* FIX: espaçamento correto entre letras e palavras */
                                            letter-spacing: 0.02em;
                                            word-spacing: 0.05em;
                                        }
                                        .header-logo {
                                            margin-bottom: 6px;
                                        }
                                        .header-logo img {
                                            height: 36px;
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
                                         * {
                                                box-sizing: border-box;
                                                margin: 0;
                                                padding: 0;
                                            }

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

                                            /* Título */
                                            .report-title {
                                                text-align: center;
                                                margin-top: 16px;
                                                margin-bottom: 16px;
                                            }

                                                .report-title h1 {
                                                    font-size: 14px;
                                                    font-weight: 700;
                                                    color: #42474A;
                                                    margin-bottom: 4px;
                                                    line-height: 1.2;
                                                }

                                                .report-title h2 {
                                                    font-size: 10px;
                                                    font-weight: 400;
                                                    color: #42474A;
                                                    line-height: 12px;
                                                }

                                            /* Tabela principal */
                                            .main-table {
                                                width: 100%;
                                                border-collapse: collapse;
                                                table-layout: fixed;
                                            }

                                                .main-table thead {
                                                    display: table-header-group;
                                                }

                                                .main-table tbody {
                                                    display: table-row-group;
                                                }

                                                .main-table th {
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

                                                .main-table thead th.col-nome {
                                                    text-align: left;
                                                    padding: 0 8px;
                                                }

                                                .main-table td {
                                                    border: 1px solid #F0F0F0;
                                                    padding: 0 8px;
                                                    text-align: center;
                                                    vertical-align: middle;
                                                    font-size: 10px;
                                                    font-weight: 400;
                                                    line-height: 12px;
                                                    color: #42474A;
                                                    background-color: #fff;
                                                }

                                                .main-table tbody tr {
                                                    page-break-inside: avoid;
                                                }

                                            /* Larguras de coluna */
                                            .col-num {
                                                width: 4.97%;
                                            }

                                            .col-nome {
                                                width: 21.31%;
                                            }

                                            .col-raca {
                                                width: 10.83%;
                                            }

                                            .col-genero {
                                                width: 11.19%;
                                            }

                                            .col-lp {
                                                width: 10.12%;
                                            }

                                            .col-resp {
                                                width: 8.31%;
                                            }

                                            .main-table tbody td.col-nome,
                                            .main-table tbody td.col-raca,
                                            .main-table tbody td.col-genero {
                                                text-align: left;
                                                padding: 8px;
                                            }

                                            .main-table td.col-resp {
                                                padding: 4px;
                                                font-weight: 500;
                                            }

                                            /* Nome do aluno e badges */
                                            .aluno-nome {
                                                font-size: 10px;
                                                font-weight: 400;
                                                line-height: 12px;
                                                color: #42474A;
                                            }

                                            .badges-container {
                                                margin-top: 4px;
                                                display: flex;
                                                flex-wrap: wrap;
                                                gap: 4px;
                                                align-items: center;
                                            }

                                            .badge-icon {
                                                height: 12px;
                                                vertical-align: middle;
                                            }

                                            /* Checkbox LP */
                                            .checkbox-lp {
                                                display: inline-block;
                                                width: 16px;
                                                height: 16px;
                                                border: 1.5px solid #BFBFC2;
                                                border-radius: 2px;
                                                background-color: #fff;
                                                vertical-align: middle;
                                            }

                                                .checkbox-lp.checked {
                                                    background-color: #6933FF;
                                                    border-color: #6933FF;
                                                    color: #fff;
                                                    font-size: 11px;
                                                    font-weight: 700;
                                                    text-align: center;
                                                    line-height: 15px;
                                                }

                                            /* Célula de resposta vazia */
                                            .resposta-vazio {
                                                color: #BFBFC2;
                                                font-weight: 500;
                                                font-size: 10px;
                                                line-height: 12px;
                                            }
                                        </style>
                                    </head>
                        <body>
                        """);

        html.Append(GerarCabecalho(dto));
        html.Append(GerarTemplate(dto));
        html.Append(GerarGrafico(ConsolidarGrafico(dto)));

        html.Append("</body></html>");

        return html.ToString();
    }

    private static GraficoSondagemDto ConsolidarGrafico(ConsultaSondagemPorTurmaDto dto)
    {
        var colunas = dto.Estudantes?
        .FirstOrDefault()?.Coluna?.ToList();

        var totaisPorOpcao = new Dictionary<int, (string Legenda, string CorFundo, string CorTexto, int Count)>();
        int totalVazio = 0;

        if (dto.Estudantes != null)
        {
            foreach (var estudante in dto.Estudantes)
            {
                foreach (var coluna in colunas)
                {
                    var col = estudante.Coluna?
                        .FirstOrDefault(c => c.IdCiclo == coluna.IdCiclo
                                          && c.QuestaoSubrespostaId == coluna.QuestaoSubrespostaId);

                    if (col?.Resposta == null) continue;

                    var opcao = col.Resposta.OpcaoRespostaId != null && col.OpcaoResposta != null
                        ? col.OpcaoResposta.FirstOrDefault(o => o.Id == col.Resposta.OpcaoRespostaId)
                        : null;

                    if (opcao == null)
                    {
                        totalVazio++;
                    }
                    else
                    {
                        if (totaisPorOpcao.ContainsKey(opcao.Id))
                        {
                            var t = totaisPorOpcao[opcao.Id];
                            totaisPorOpcao[opcao.Id] = (t.Legenda, t.CorFundo, t.CorTexto, t.Count + 1);
                        }
                        else
                        {
                            totaisPorOpcao[opcao.Id] = (
                                opcao.DescricaoOpcaoResposta ?? opcao.Legenda ?? "?",
                                opcao.CorFundo ?? "#BFBFC2",
                                opcao.CorTexto ?? "#fff",
                                1
                            );
                        }
                    }
                }
            }
        }

        var barras = totaisPorOpcao.Values
            .OrderByDescending(b => b.Count)
            .Select(b => new GraficoBarraDto { Legenda = b.Legenda, CorFundo = b.CorFundo, CorTexto = b.CorTexto, Quantidade = b.Count })
            .ToList();

        if (totalVazio > 0)
            barras.Add(new GraficoBarraDto { Legenda = "Vazio", CorFundo = "#E0E0E0", CorTexto = "#42474A", Quantidade = totalVazio });

        var grafico = new GraficoSondagemDto
        {
            Titulo = "Gráfico da Sondagem",
            Subtitulo = dto.TituloTabelaRespostas,
            Barras = barras
        };

        return grafico;
    }

    private static string GerarCabecalho(ConsultaSondagemPorTurmaDto dto)
    {
        var sb = new StringBuilder();

        sb.Append($@"
                        <div class=""header-logo"">
                                        <img src=""{SmeConstants.Logo_PrefSP_Horizontal}"" alt=""Prefeitura de São Paulo"" />
                                      </div>
                                      <table class=""meta-table"">
                                        <tr>
                                          <td><strong>Ano letivo:</strong> {dto.AnoLetivo}</td>
                                          <td><strong>DRE:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Dre)}</td>
                                          <td><strong>Semestre:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Semestre)}</td>
                                          <td><strong>Turma:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Turma)}</td>
                                        </tr>
                                        <tr>
                                          <td colspan=""4""><strong>Unidade Educacional:</strong> {System.Web.HttpUtility.HtmlEncode(dto.UnidadeEducacional)}</td>
                                        </tr>
                                        <tr>
                                          <td colspan=""2""><strong>Modalidade:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Modalidade)}</td>
                                          <td><strong>Proficiência:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Proficiencia)}</td>
                                          <td><strong>Data de impressão:</strong> {dto.DataImpressao:dd/MM/yyyy}</td>
                                        </tr>
                                        <tr>
                                          <td colspan=""4""><strong>Usuário:</strong> {System.Web.HttpUtility.HtmlEncode(dto.Usuario)}</td>
                                        </tr>
                                      </table>
                  ");

        return sb.ToString();
    }

    public static string GerarTemplate(ConsultaSondagemPorTurmaDto model)
    {

        var colunas = model.Estudantes?.FirstOrDefault()?.Coluna?.ToList()
                      ?? new List<ColunaDto>();

        int nColunas = colunas.Count > 0 ? colunas.Count : 1;
        string larguraResp = (41.58 / nColunas).ToString("F2", CultureInfo.InvariantCulture) + "%";

        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"report-title\">");
        sb.AppendLine($"    <h1>Relatório da Sondagem</h1>");
        sb.AppendLine($"    <h2>{model.TituloTabelaRespostas}</h2>");
        sb.AppendLine("</div>");

        sb.AppendLine("<table class=\"main-table\">");

        sb.AppendLine("    <colgroup>");
        sb.AppendLine("        <col style=\"width: 4.97%;\" />");
        sb.AppendLine("        <col style=\"width: 23.09%;\" />");
        sb.AppendLine("        <col style=\"width: 10.12%;\" />");
        sb.AppendLine("        <col style=\"width: 10.12%;\" />");
        sb.AppendLine("        <col style=\"width: 10.12%;\" />");
        for (int i = 0; i < nColunas; i++)
        {
            sb.AppendLine($"        <col style=\"width: {larguraResp};\" />");
        }
        sb.AppendLine("    </colgroup>");

        sb.AppendLine("    <thead>");
        sb.AppendLine("        <tr>");
        sb.AppendLine("            <th rowspan=\"2\" class=\"col-num\">N°</th>");
        sb.AppendLine("            <th rowspan=\"2\" class=\"col-nome\">Nome</th>");
        sb.AppendLine("            <th rowspan=\"2\" class=\"col-raca\">Raça</th>");
        sb.AppendLine("            <th rowspan=\"2\" class=\"col-genero\">Gênero</th>");
        sb.AppendLine("            <th rowspan=\"2\" class=\"col-lp\">LP como 2° língua?</th>");
        sb.AppendLine($"            <th colspan=\"{nColunas}\">{model.TituloTabelaRespostas}</th>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("        <tr>");
        foreach (var col in colunas)
        {
            sb.AppendLine($"            <th class=\"col-resp\">{col.DescricaoColuna}</th>");
        }
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </thead>");

        sb.AppendLine("    <tbody>");
        if (model.Estudantes != null)
        {
            foreach (var estudante in model.Estudantes)
            {
                sb.AppendLine("        <tr>");
                sb.AppendLine($"            <td class=\"col-num\">{estudante.NumeroAlunoChamada}</td>");

                sb.AppendLine("            <td class=\"col-nome aluno-nome\">");
                sb.AppendLine($"                {estudante.NomeRelatorio}");
                if (estudante.Aee || estudante.Pap || estudante.PossuiDeficiencia)
                {
                    sb.AppendLine("                <div class=\"badges-container\">");
                    if (estudante.Aee) sb.AppendLine($"                    <img src=\"{SmeConstants.Logo_AEE}\" class=\"badge-icon\" alt=\"AEE\" />");
                    if (estudante.Pap) sb.AppendLine($"                    <img src=\"{SmeConstants.Logo_PAP}\" class=\"badge-icon\" alt=\"PAP\" />");
                    if (estudante.PossuiDeficiencia) sb.AppendLine($"                    <img src=\"{SmeConstants.Logo_Acessibilidade}\" class=\"badge-icon\" alt=\"Deficiência\" />");
                    sb.AppendLine("                </div>");
                }
                sb.AppendLine("            </td>");

                sb.AppendLine($"            <td class=\"col-raca\">{estudante.Raca}</td>");
                sb.AppendLine($"            <td class=\"col-genero\">{estudante.Genero}</td>");

                string checkLp = estudante.LinguaPortuguesaSegundaLingua
                    ? "<span class=\"checkbox-lp checked\">&#10003;</span>"
                    : "<span class=\"checkbox-lp\"></span>";
                sb.AppendLine($"            <td class=\"col-lp\">{checkLp}</td>");

                foreach (var colCabecalho in colunas)
                {
                    sb.Append(GerarTdResposta(estudante, colCabecalho));
                }

                sb.AppendLine("        </tr>");
            }
        }
        sb.AppendLine("    </tbody>");
        sb.AppendLine("</table>");

        return sb.ToString();
    }

    private static string GerarTdResposta(EstudanteDto estudante, ColunaDto colRef)
    {
        var colEstudante = estudante.Coluna?.FirstOrDefault(c =>
            c.IdCiclo == colRef.IdCiclo && c.QuestaoSubrespostaId == colRef.QuestaoSubrespostaId);

        if (colEstudante?.Resposta == null)
            return "            <td class=\"col-resp\"><span class=\"resposta-vazio\">-</span></td>";

        var opcao = colEstudante.OpcaoResposta?.FirstOrDefault(o => o.Id == colEstudante.Resposta.OpcaoRespostaId);

        if (opcao == null)
            return "            <td class=\"col-resp\"><span class=\"resposta-vazio\">Vazio</span></td>";

        string style = $"style=\"background-color: {opcao.CorFundo ?? "transparent"}; color: {opcao.CorTexto ?? "#333"};\"";
        string texto = opcao.DescricaoOpcaoResposta ?? opcao.Legenda;

        return $"            <td class=\"col-resp\" {style}>{texto}</td>";
    }

    public static string GerarGrafico(GraficoSondagemDto model)
    {
        if (model.Barras == null || !model.Barras.Any())
            return string.Empty;

        var sb = new StringBuilder();
        var ptBR = new CultureInfo("pt-BR");

        var barras = model.Barras;
        int maxValor = barras.Max(b => b.Quantidade);
        if (maxValor == 0) maxValor = 1;

        int stepCount = 7;
        double rawStep = (double)maxValor / (stepCount - 1);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep == 0 ? 1 : rawStep)));
        double niceStep = Math.Ceiling(rawStep / magnitude) * magnitude;

        int yMax = (int)(niceStep * (stepCount - 1));
        int yStep = (int)niceStep;

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

        sb.AppendLine("<div style=\"page-break-before: always; padding-top: 16px;\">");

        sb.AppendLine("    <div style=\"text-align:center; margin-bottom:20px;\">");
        sb.AppendLine($"        <h1 style=\"font-size:14px; font-weight:700; color:#42474A; margin-bottom:4px;\">{model.Titulo}</h1>");
        sb.AppendLine($"        <h2 style=\"font-size:10px; font-weight:400; color:#42474A;\">{model.Subtitulo}</h2>");
        sb.AppendLine("    </div>");

        sb.AppendLine("    <table style=\"width:auto; border-collapse:collapse; border:none;\">");
        sb.AppendLine("        <tr>");

        sb.AppendLine("            <td style=\"width:16px; padding:0; border:none; vertical-align:middle; text-align:center;\">");
        sb.AppendLine($"                <img src=\"{svgYLabel}\" style=\"width:16px; height:{chartHeight}px; display:block;\" />");
        sb.AppendLine("            </td>");

        sb.AppendLine("            <td style=\"width:38px; padding:0; border:none; vertical-align:top;\">");
        for (int i = stepCount - 1; i >= 0; i--)
        {
            var nivelValor = yStep * i;
            sb.AppendLine($"                <div style=\"height:{rowHeight}px; width:100%;\">");
            sb.AppendLine($"                    <table style=\"width:100%; height:{rowHeight}px; border-collapse:collapse;\">");
            sb.AppendLine("                        <tr>");
            sb.AppendLine($"                            <td style=\"vertical-align:bottom; text-align:right; padding:0 4px 1px 0; font-size:8px; color:#42474A; white-space:nowrap; border:none;\">");
            sb.AppendLine($"                                {nivelValor.ToString("N0", ptBR)}");
            sb.AppendLine("                            </td>");
            sb.AppendLine("                        </tr>");
            sb.AppendLine("                    </table>");
            sb.AppendLine("                </div>");
        }
        sb.AppendLine("            </td>");
        sb.AppendLine($"            <td style=\"width:{areaWidth}px; padding:0; border:none; vertical-align:top;\">");

        for (int i = stepCount - 1; i >= 0; i--)
        {
            var border = (i == 0) ? "border-bottom:2px solid #9E9E9E;" : "border-bottom:1px solid #E0E0E0;";
            sb.AppendLine($"                <div style=\"height:{rowHeight}px; width:{areaWidth}px; {border}\"></div>");
        }

        sb.AppendLine($"                <div style=\"margin-top:-{chartHeight}px; white-space:nowrap; padding-left:{barPadLeft}px;\">");
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
                sb.AppendLine($"                                {barra.Quantidade.ToString("N0", ptBR)}");
                sb.AppendLine("                            </span>");
                sb.AppendLine("                        </div>");
            }
            sb.AppendLine("                    </div>");
        }
        sb.AppendLine("                </div>");

        sb.AppendLine($"                <div style=\"white-space:nowrap; padding-left:{barPadLeft}px; margin-top:4px;\">");
        foreach (var barra in barras)
        {
            sb.AppendLine($"                    <div style=\"display:inline-block; width:{barWidth}px; margin-right:{barMargin}px; text-align:center; font-size:8px; color:#42474A; white-space:normal; vertical-align:top;\">");
            sb.AppendLine($"                        {barra.Legenda}");
            sb.AppendLine("                    </div>");
        }
        sb.AppendLine("                </div>");

        sb.AppendLine("                <div style=\"text-align:center; font-size:9px; font-weight:700; color:#42474A; margin-top:6px;\">");
        sb.AppendLine("                    Opções de respostas");
        sb.AppendLine("                </div>");

        sb.AppendLine("            </td>");
        sb.AppendLine("        </tr>");
        sb.AppendLine("    </table>");
        sb.AppendLine("</div>");

        return sb.ToString();
    }
}
