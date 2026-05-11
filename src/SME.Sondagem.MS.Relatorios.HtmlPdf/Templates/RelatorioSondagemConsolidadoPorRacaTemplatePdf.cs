using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;
using System.Linq;

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
        AdicionarRacasDasRespostas(questao.Respostas, encontradas);
        AdicionarRacasDosTotais(questao.TotaisPorRaca, encontradas);
        return encontradas;
    }

    private static void AdicionarRacasDasRespostas(
        IEnumerable<RelatorioConsolidadoRespostaDto>? respostas,
        HashSet<string> encontradas)
    {
        if (respostas == null) return;

        foreach (var raca in respostas
            .SelectMany(resposta => resposta.Racas ?? Enumerable.Empty<RelatorioConsolidadoRacaDto>())
            .Where(raca => !string.IsNullOrWhiteSpace(raca.Raca)))
        {
            encontradas.Add(raca.Raca);
        }
    }

    private static void AdicionarRacasDosTotais(
        IEnumerable<RelatorioConsolidadoRacaDto>? totaisPorRaca,
        HashSet<string> encontradas)
    {
        if (totaisPorRaca == null) return;
        foreach (var raca in from raca in totaisPorRaca
                             where !string.IsNullOrWhiteSpace(raca.Raca)
                             select raca)
        {
            encontradas.Add(raca.Raca);
        }
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
