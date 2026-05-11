using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;
using System.Linq;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorBimestreTemplatePdf
    : RelatorioSondagemConsolidadoDemograficoTemplatePdfBase,
      IRelatorioSondagemConsolidadoPorBimestreTemplatePdf
{
    protected override string CssClasseColuna => "col-bimestre";

    protected override string GerarLinhaMetadadoDemografico(RelatorioConsolidadoSondagemDto dto) =>
        $"<td colspan=\"2\"><strong>Bimestre:</strong> {HttpUtility.HtmlEncode(dto.Bimestre)}</td>";

    protected override List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto) =>
        dto.BimestresDisponiveis?
            .Select(b => b.Descricao)
            .ToList() ?? [];

    protected override string FormatarCabecalhoColuna(string nomeColuna) => nomeColuna;

    protected override HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao)
    {
        var encontradas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AdicionarBimestresDasRespostas(questao.Respostas, encontradas);
        AdicionarBimestresDosTotais(questao.TotaisPorBimestre, encontradas);
        return encontradas;
    }

    private static void AdicionarBimestresDasRespostas(
        IEnumerable<RelatorioConsolidadoRespostaDto>? respostas,
        HashSet<string> encontradas)
    {
        if (respostas == null) return;

        foreach (var bimestre in respostas
            .SelectMany(resposta => resposta.Bimestres ?? Enumerable.Empty<RelatorioConsolidadoBimestreDto>())
            .Where(b => !string.IsNullOrWhiteSpace(b.Bimestre)))
        {
            encontradas.Add(bimestre.Bimestre);
        }
    }

    private static void AdicionarBimestresDosTotais(
        IEnumerable<RelatorioConsolidadoBimestreDto>? totaisPorBimestre,
        HashSet<string> encontradas)
    {
        if (totaisPorBimestre == null) return;
        foreach (var bimestre in from bimestre in totaisPorBimestre
                                 where !string.IsNullOrWhiteSpace(bimestre.Bimestre)
                                 select bimestre)
        {
            encontradas.Add(bimestre.Bimestre);
        }
    }

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(
        RelatorioConsolidadoRespostaDto resposta) =>
        (resposta.Bimestres ?? [])
            .Where(b => !string.IsNullOrWhiteSpace(b.Bimestre))
            .GroupBy(b => b.Bimestre, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(
        RelatorioConsolidadoQuestaoDto questao) =>
        (questao.TotaisPorBimestre ?? [])
            .Where(b => !string.IsNullOrWhiteSpace(b.Bimestre))
            .GroupBy(b => b.Bimestre, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (g.First().Quantidade, g.First().Percentual),
                StringComparer.OrdinalIgnoreCase);

    protected override bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao) =>
        questao.TotaisPorBimestre != null && questao.TotaisPorBimestre.Any();
}
