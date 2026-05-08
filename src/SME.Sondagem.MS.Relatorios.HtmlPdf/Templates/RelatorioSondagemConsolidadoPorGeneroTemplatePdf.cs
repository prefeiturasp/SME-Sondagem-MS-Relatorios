using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorGeneroTemplatePdf : RelatorioSondagemConsolidadoDemograficoTemplatePdfBase, IRelatorioSondagemConsolidadoPorGeneroTemplatePdf
{
    protected override string CssClasseColuna => "col-genero";

    protected override string CssExtraSecaoTabelaH3 =>
        "background-color: #FAFAFA;\n                            padding: 8px;\n                            border: 1px solid #F0F0F0;\n                            border-bottom: none;";

    protected override string GerarLinhaMetadadoDemografico(RelatorioConsolidadoSondagemDto dto) =>
        $"<td colspan=\"2\"><strong>Gênero:</strong> {HttpUtility.HtmlEncode(dto.Genero)}</td>";

    protected override List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto) =>
        dto.GenerosDisponiveis?
            .Select(g => g.Descricao)
            .ToList() ?? [];

    protected override string FormatarCabecalhoColuna(string nomeColuna) =>
        $"Gênero: {nomeColuna}";

    protected override HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao)
    {
        var encontrados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AdicionarDeLista(IEnumerable<RelatorioConsolidadoGeneroDto>? lista)
        {
            if (lista == null) return;
            foreach (var item in lista)
            {
                var rotulo = ObterRotuloGenero(item);
                if (!string.IsNullOrWhiteSpace(rotulo))
                    encontrados.Add(rotulo);
            }
        }

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas)
                AdicionarDeLista(resposta.Generos);
        }

        AdicionarDeLista(questao.TotaisPorGenero);

        return encontrados;
    }

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta) =>
        (resposta.Generos ?? [])
            .Select(g => (Chave: ObterRotuloGenero(g), g.Quantidade, g.Percentual))
            .Where(x => !string.IsNullOrWhiteSpace(x.Chave))
            .GroupBy(x => x.Chave, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao) =>
        (questao.TotaisPorGenero ?? [])
            .Select(g => (Chave: ObterRotuloGenero(g), g.Quantidade, g.Percentual))
            .Where(x => !string.IsNullOrWhiteSpace(x.Chave))
            .GroupBy(x => x.Chave, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao) =>
        questao.TotaisPorGenero != null && questao.TotaisPorGenero.Any();

    private static string ObterRotuloGenero(RelatorioConsolidadoGeneroDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Genero))
            return dto.Genero.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Sigla))
            return dto.Sigla.Trim();
        return string.Empty;
    }
}
