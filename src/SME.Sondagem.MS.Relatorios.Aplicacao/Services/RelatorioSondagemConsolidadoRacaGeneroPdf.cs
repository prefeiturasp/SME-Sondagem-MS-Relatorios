using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoRacaGeneroPdf : IRelatorioSondagemConsolidadoRacaGeneroPdf
{
    private readonly IReportConverter _reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf _template;

    public RelatorioSondagemConsolidadoRacaGeneroPdf(
        IReportConverter reportConverter,
        IServicoArmazenamentoMinio servicoArmazenamentoMinio,
        IRelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf template)
    {
        _reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _template = template;
    }

    public async Task<string> Executar(RelatorioConsolidadoSondagemDto consultaSondagemPorTurmaDto)
    {
        var relatorioHtml = _template.GerarHtml(consultaSondagemPorTurmaDto);
        var cabecalhoWk = _template.GerarHtmlDocumentoCabecalhoWk(consultaSondagemPorTurmaDto);

        byte[] pdfBytes = _reportConverter.GerarPdfEmMemoria(relatorioHtml, cabecalhoWk);
        string nomeArquivo = $"Relatorio/{consultaSondagemPorTurmaDto.CodigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
