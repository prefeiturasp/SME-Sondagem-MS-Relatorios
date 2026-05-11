using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoQuestaoPdf : IRelatorioSondagemConsolidadoQuestaoPdf
{
    private readonly IReportConverter _reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemConsolidadoPorQuestaoTemplatePdf _template;

    public RelatorioSondagemConsolidadoQuestaoPdf(
        IReportConverter reportConverter,
        IServicoArmazenamentoMinio servicoArmazenamentoMinio,
        IRelatorioSondagemConsolidadoPorQuestaoTemplatePdf template)
    {
        _reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _template = template;
    }

    public async Task<string> Executar(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        var relatorioHtml = _template.GerarHtml(dadosRelatorio);

        byte[] pdfBytes = _reportConverter.GerarPdfEmMemoria(relatorioHtml);
        string nomeArquivo = $"Relatorio/{dadosRelatorio.CodigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
