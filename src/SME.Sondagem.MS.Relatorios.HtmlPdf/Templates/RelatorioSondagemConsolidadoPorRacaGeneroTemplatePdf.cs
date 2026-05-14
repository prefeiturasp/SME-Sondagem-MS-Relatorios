using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Text;
using System.Web;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf : RelatorioSondagemConsolidadoDemograficoTemplatePdfBase, IRelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf
{
    private const string FechaDiv = "</div>";

    protected override string CssClasseColuna => "col-raca";

    protected override string CssExtraSecaoTabelaH3 =>
        "background-color: #FAFAFA;\n                            padding: 8px;\n                            border: 1px solid #F0F0F0;\n                            border-bottom: none;";

    protected override List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto) =>
        dto.RacasDisponiveis?.Select(r => r.Descricao).ToList() ?? [];

    protected override HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao) => [];

    protected override string FormatarCabecalhoColuna(string nomeColuna) => nomeColuna;

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta) => [];

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao) => [];

    protected override bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao) => false;

    protected override string GerarBlocosRelatorio(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();

        if (dto.Questoes == null)
            return sb.ToString();

        var preferenciaRacas = ObterOrdemColunas(dto);
        var generosOrdenados = ObterOrdemGeneros(dto);

        foreach (var questao in dto.Questoes)
        {
            foreach (var genero in generosOrdenados)
            {
                var colunas = ObterColunasRacaOrdenadasParaGenero(questao, preferenciaRacas, genero);
                if (colunas.Count == 0)
                    continue;

                sb.Append("<div class=\"bloco-relatorio\">");
                sb.Append(GerarSecaoTabelaPorGenero(questao, colunas, genero));
                sb.AppendLine(FechaDiv);
            }
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

    private static List<string> ObterOrdemGeneros(RelatorioConsolidadoSondagemDto dto)
    {
        var catalogo = dto.GenerosDisponiveis?
            .Select(g => g.Descricao)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList() ?? [];

        var presentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var q in dto.Questoes ?? [])
            ColetarGenerosPresentesNaQuestao(q, presentes);

        var resultado = (from g in catalogo
                         where presentes.Contains(g)
                         select g).ToList();
        resultado.AddRange(from g in presentes.OrderBy(x => x)
                           where !resultado.Contains(g, StringComparer.OrdinalIgnoreCase)
                           select g);
        return resultado;
    }

    private static void ColetarGenerosPresentesNaQuestao(RelatorioConsolidadoQuestaoDto questao, HashSet<string> presentes)
    {
        if (questao.TotaisPorGeneroComRacas != null)
        {
            foreach (var t in questao.TotaisPorGeneroComRacas.Where(x => !string.IsNullOrWhiteSpace(x.Genero)))
                presentes.Add(t.Genero.Trim());
        }

        if (questao.Respostas == null)
            return;

        foreach (var genero in questao.Respostas
            .Where(r => r.GenerosComRacas != null)
            .SelectMany(r => r.GenerosComRacas ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Genero)))
        {
            presentes.Add(genero.Genero.Trim());
        }
    }

    private static List<string> ObterColunasRacaOrdenadasParaGenero(
        RelatorioConsolidadoQuestaoDto questao,
        List<string> preferenciaRacas,
        string genero)
    {
        var encontradas = ColetarRacasEncontradasParaGenero(questao, genero);

        var ordenadas = preferenciaRacas
            .Where(c => encontradas.Contains(c))
            .ToList();

        ordenadas.AddRange(
            from coluna in encontradas.OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            where !ordenadas.Contains(coluna, StringComparer.OrdinalIgnoreCase)
            select coluna);

        return ordenadas;
    }

    private static HashSet<string> ColetarRacasEncontradasParaGenero(RelatorioConsolidadoQuestaoDto questao, string genero)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AdicionarRacas(IEnumerable<RelatorioConsolidadoRacaDto>? racas)
        {
            if (racas == null) return;
            foreach (var r in racas.Where(x => !string.IsNullOrWhiteSpace(x.Raca)))
                set.Add(r.Raca);
        }

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas)
            {
                var bloco = resposta.GenerosComRacas?.FirstOrDefault(g =>
                    g.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
                AdicionarRacas(bloco?.Racas);
            }
        }

        var totaisGenero = questao.TotaisPorGeneroComRacas?
            .FirstOrDefault(t => t.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
        AdicionarRacas(totaisGenero?.Racas);

        return set;
    }

    private string GerarSecaoTabelaPorGenero(RelatorioConsolidadoQuestaoDto questao, List<string> colunas, string genero)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"secao-tabela\">");
        sb.AppendLine($"    <h3>Gênero: {HttpUtility.HtmlEncode(genero)}</h3>");
        sb.AppendLine("    <table class=\"main-table\">");

        sb.AppendLine(GerarColgroup(colunas.Count));
        sb.AppendLine(GerarCabecalhoTabelaRaca(questao, colunas));
        sb.AppendLine(GerarCorpoTabelaPorGenero(questao, colunas, genero));

        sb.AppendLine("    </table>");
        sb.AppendLine(FechaDiv);
        return sb.ToString();
    }

    private string GerarCabecalhoTabelaRaca(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
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

    private string GerarCorpoTabelaPorGenero(RelatorioConsolidadoQuestaoDto questao, List<string> colunas, string genero)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        <tbody>");

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas
                .Where(r => r != null)
                .OrderBy(r => r.Ordem))
                sb.AppendLine(GerarLinhaRespostaPorGenero(resposta, colunas, genero));
        }

        if (PossuiTotaisPorGenero(questao, genero))
            sb.AppendLine(GerarLinhaTotalPorGenero(questao, colunas, genero));

        sb.AppendLine("        </tbody>");
        return sb.ToString();
    }

    private static bool PossuiTotaisPorGenero(RelatorioConsolidadoQuestaoDto questao, string genero)
    {
        var bloco = questao.TotaisPorGeneroComRacas?
            .FirstOrDefault(t => t.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
        return bloco?.Racas != null && bloco.Racas.Any();
    }

    private string GerarLinhaRespostaPorGenero(RelatorioConsolidadoRespostaDto resposta, List<string> colunas, string genero)
    {
        var sb = new StringBuilder();
        bool isSemPreenchimento = resposta.Resposta?.Trim()
            .Equals("Sem preenchimento", StringComparison.OrdinalIgnoreCase) == true;

        var classeLinha = isSemPreenchimento ? "row-sem-preenchimento" : string.Empty;
        sb.AppendLine($"            <tr class=\"{classeLinha}\">");
        sb.AppendLine(GerarBadgeNivel(resposta));

        var dic = MontarDicionarioRacasDoGeneroNaResposta(resposta, genero);
        AppendCelulasValor(sb, colunas, dic);

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    private string GerarLinhaTotalPorGenero(RelatorioConsolidadoQuestaoDto questao, List<string> colunas, string genero)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            <tr class=\"row-total\">");
        sb.AppendLine("                <td>Total</td>");

        var dic = MontarDicionarioTotaisPorGenero(questao, genero);
        AppendCelulasValor(sb, colunas, dic, vazioTexto: "-");

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    private static Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioRacasDoGeneroNaResposta(
        RelatorioConsolidadoRespostaDto resposta,
        string genero)
    {
        var bloco = resposta.GenerosComRacas?
            .FirstOrDefault(g => g.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
        return (bloco?.Racas ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Raca))
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotaisPorGenero(
        RelatorioConsolidadoQuestaoDto questao,
        string genero)
    {
        var bloco = questao.TotaisPorGeneroComRacas?
            .FirstOrDefault(t => t.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
        return (bloco?.Racas ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Raca))
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);
    }
}
