using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Text;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;

public class RelatorioSondagemConsolidadoPorQuestaoTemplatePdf : RelatorioSondagemConsolidadoDemograficoTemplatePdfBase, IRelatorioSondagemConsolidadoPorQuestaoTemplatePdf
{
    protected override string CssClasseColuna => "col-valor";

    protected override bool ExibirLinhaFiltroDemografico => false;

    protected override bool ExibirTituloQuestao => false;

    protected override string ObterRotuloColunaNivel(RelatorioConsolidadoQuestaoDto questao) => questao.QuestaoNome;

    protected override List<string> ObterOrdemColunas(RelatorioConsolidadoSondagemDto dto) => ["Estudantes", "%"];

    protected override string FormatarCabecalhoColuna(string nomeColuna) => nomeColuna;

    protected override HashSet<string> ColetarColunasEncontradas(RelatorioConsolidadoQuestaoDto questao) => ["Estudantes", "%"];

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioResposta(RelatorioConsolidadoRespostaDto resposta) => [];

    protected override Dictionary<string, (int Quantidade, double Percentual)> MontarDicionarioTotais(RelatorioConsolidadoQuestaoDto questao) => [];

    protected override bool PossuiTotais(RelatorioConsolidadoQuestaoDto questao) => true;

    protected override void AppendCelulasValor(
        StringBuilder sb,
        List<string> colunas,
        Dictionary<string, (int Quantidade, double Percentual)> dic,
        string vazioTexto = "Vazio")
    {
        // Esta sobrecarga é chamada via Reflection ou Dispatch se não tomarmos cuidado, 
        // mas o C# resolverá para a implementação da instância.
        // No entanto, o fluxo da base passa o dicionário. Para "Por Questão", ignoramos o dicionário
        // e usamos os dados da resposta/questão que capturaremos no contexto se necessário, 
        // ou mudamos a estratégia.
    }

    // Como o fluxo da base chama AppendCelulasValor passando o dicionário, mas para "Por Questão" 
    // os dados não estão em um dicionário demográfico, vou sobrecarregar as linhas.

    protected override string GerarLinhaResposta(RelatorioConsolidadoRespostaDto resposta, List<string> colunas)
    {
        var sb = new StringBuilder();
        bool isSemPreenchimento = resposta.Resposta?.Trim()
            .Equals("Sem preenchimento", StringComparison.OrdinalIgnoreCase) == true;

        var classeLinha = isSemPreenchimento ? "row-sem-preenchimento" : string.Empty;
        sb.AppendLine($"            <tr class=\"{classeLinha}\">");
        sb.AppendLine(GerarBadgeNivel(resposta));

        sb.AppendLine($"                <td>{FormatarValor(resposta.Total)}</td>");
        sb.AppendLine($"                <td>{FormatarPercentual(resposta.Percentual)}</td>");

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    protected override string GerarLinhaTotal(RelatorioConsolidadoQuestaoDto questao, List<string> colunas)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            <tr class=\"row-total\">");
        sb.AppendLine("                <td>Total</td>");

        sb.AppendLine($"                <td>{FormatarValor(questao.TotalEstudantes)}</td>");
        sb.AppendLine($"                <td>{FormatarPercentual(questao.PercentualTotal)}</td>");

        sb.AppendLine("            </tr>");
        return sb.ToString();
    }

    private static string FormatarValor(int quantidade)
    {
        if (quantidade <= 0)
            return "<span class=\"cell-vazio\">Vazio</span>";

        return $"<span class=\"cell-valor\">{quantidade.ToString("N0", PtBr)}</span>";
    }

    private static string FormatarPercentual(double percentual)
    {
        if (percentual <= 0)
            return "<span class=\"cell-vazio\">Vazio</span>";

        return $"<span class=\"cell-valor\">{percentual.ToString("N2", PtBr)}%</span>";
    }
}
