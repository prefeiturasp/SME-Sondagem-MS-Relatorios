using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorRacaTemplatePdf : RelatorioSondagemConsolidadoDemograficoTemplatePdfBase, IRelatorioSondagemConsolidadoPorRacaTemplatePdf
{
    protected override string CssClasseColuna => "col-raca";

    protected override string GerarLinhaMetadadoDemografico(RelatorioConsolidadoSondagemDto dto) =>
        $"<td colspan=\"2\"><strong>Raça:</strong> {HttpUtility.HtmlEncode(dto.Raca)}</td>";

    protected override List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto) =>
        dto.RacasDisponiveis?
            .Select(r => r.Descricao)
            .ToList() ?? [];

    protected override string FormatarCabecalhoColuna(string nomeColuna) => nomeColuna;

    protected override HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao)
    {
        var encontradas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas)
            {
                if (resposta.Racas == null) continue;
                foreach (var raca in resposta.Racas)
                {
                    if (!string.IsNullOrWhiteSpace(raca.Raca))
                        encontradas.Add(raca.Raca);
                }
            }
        }

        if (questao.TotaisPorRaca != null)
        {
            foreach (var raca in questao.TotaisPorRaca)
            {
                if (!string.IsNullOrWhiteSpace(raca.Raca))
                    encontradas.Add(raca.Raca);
            }
        }

        return encontradas;
    }

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta) =>
        (resposta.Racas ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Raca))
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao) =>
        (questao.TotaisPorRaca ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r.Raca))
            .GroupBy(r => r.Raca, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao) => questao.TotaisPorRaca != null && questao.TotaisPorRaca.Any();
}
