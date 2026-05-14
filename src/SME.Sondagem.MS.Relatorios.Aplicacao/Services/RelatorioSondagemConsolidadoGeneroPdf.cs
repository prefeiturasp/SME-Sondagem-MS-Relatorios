using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoGeneroPdf : IRelatorioSondagemConsolidadoGeneroPdf
{
    private readonly IReportConverter reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemConsolidadoPorGeneroTemplatePdf _relatorioSondagemConsolidadoPorGeneroTemplatePdf;

    public RelatorioSondagemConsolidadoGeneroPdf(IReportConverter reportConverter,
                                                 IServicoArmazenamentoMinio servicoArmazenamentoMinio,
                                                 IRelatorioSondagemConsolidadoPorGeneroTemplatePdf relatorioSondagemConsolidadoPorGeneroTemplatePdf)
    {
        this.reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _relatorioSondagemConsolidadoPorGeneroTemplatePdf = relatorioSondagemConsolidadoPorGeneroTemplatePdf;
    }

    public async Task<string> Executar(RelatorioConsolidadoSondagemDto consultaSondagemPorTurmaDto)
    {
        var relatorioHtml = _relatorioSondagemConsolidadoPorGeneroTemplatePdf.GerarHtml(consultaSondagemPorTurmaDto);
        var cabecalhoWk = _relatorioSondagemConsolidadoPorGeneroTemplatePdf.GerarHtmlDocumentoCabecalhoWk(consultaSondagemPorTurmaDto);

        byte[] pdfBytes = reportConverter.GerarPdfEmMemoria(relatorioHtml, cabecalhoWk);
        string nomeArquivo = $"Relatorio/{consultaSondagemPorTurmaDto.CodigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
