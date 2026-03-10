using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaPdf : IRelatorioSondagemQuestionarioPorTurmaPdf
{
    private readonly IReportConverter reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;

    public RelatorioSondagemQuestionarioPorTurmaPdf(IReportConverter reportConverter, IServicoArmazenamentoMinio servicoArmazenamentoMinio)
    {
        this.reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
    }

    public async Task<string> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao)
    {
        var relatorioHtml = RelatorioSondagemQuestionarioPorTurmaTemplate.GerarHtml(consultaSondagemPorTurmaDto);

        byte[] pdfBytes = reportConverter.GerarPdfEmMemoria(relatorioHtml);
        string nomeArquivo = $"Relatorio_{codigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
