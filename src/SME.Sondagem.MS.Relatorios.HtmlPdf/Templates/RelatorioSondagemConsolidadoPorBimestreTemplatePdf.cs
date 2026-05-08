using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;

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

        if (questao.Respostas != null)
        {
            foreach (var resposta in questao.Respostas)
            {
                if (resposta.Bimestres == null) continue;
                foreach (var bimestre in resposta.Bimestres)
                {
                    if (!string.IsNullOrWhiteSpace(bimestre.Bimestre))
                        encontradas.Add(bimestre.Bimestre);
                }
            }
        }

        if (questao.TotaisPorBimestre != null)
        {
            foreach (var bimestre in questao.TotaisPorBimestre)
            {
                if (!string.IsNullOrWhiteSpace(bimestre.Bimestre))
                    encontradas.Add(bimestre.Bimestre);
            }
        }

        return encontradas;
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
