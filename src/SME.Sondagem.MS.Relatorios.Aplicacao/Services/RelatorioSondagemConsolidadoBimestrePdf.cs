using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoBimestrePdf : IRelatorioSondagemConsolidadoBimestrePdf
{
    private readonly IReportConverter _reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemConsolidadoPorBimestreTemplatePdf _template;

    public RelatorioSondagemConsolidadoBimestrePdf(
        IReportConverter reportConverter,
        IServicoArmazenamentoMinio servicoArmazenamentoMinio,
        IRelatorioSondagemConsolidadoPorBimestreTemplatePdf template)
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
