using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoRacaPdf : IRelatorioSondagemConsolidadoRacaPdf
{
    private readonly IReportConverter reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemConsolidadoPorRacaTemplatePdf _relatorioSondagemConsolidadoPorRacaTemplatePdf;

    public RelatorioSondagemConsolidadoRacaPdf(IReportConverter reportConverter, IServicoArmazenamentoMinio servicoArmazenamentoMinio, IRelatorioSondagemConsolidadoPorRacaTemplatePdf relatorioSondagemConsolidadoPorRacaTemplatePdf)
    {
        this.reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _relatorioSondagemConsolidadoPorRacaTemplatePdf = relatorioSondagemConsolidadoPorRacaTemplatePdf;
    }

    public async Task<string> Executar(RelatorioConsolidadoSondagemDto consultaSondagemPorTurmaDto)
    {
        var relatorioHtml = _relatorioSondagemConsolidadoPorRacaTemplatePdf.GerarHtml(consultaSondagemPorTurmaDto);
        var cabecalhoWk = _relatorioSondagemConsolidadoPorRacaTemplatePdf.GerarHtmlDocumentoCabecalhoWk(consultaSondagemPorTurmaDto);

        byte[] pdfBytes = reportConverter.GerarPdfEmMemoria(relatorioHtml, cabecalhoWk);
        string nomeArquivo = $"Relatorio/{consultaSondagemPorTurmaDto.CodigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
