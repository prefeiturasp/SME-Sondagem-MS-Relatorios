using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaPdf : IRelatorioSondagemQuestionarioPorTurmaPdf
{
    private readonly IReportConverter reportConverter;
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemQuestionarioPorTurmaTemplatePdf _relatorioSondagemQuestionarioPorTurmaTemplatePdf;

    public RelatorioSondagemQuestionarioPorTurmaPdf(IReportConverter reportConverter, IServicoArmazenamentoMinio servicoArmazenamentoMinio, IRelatorioSondagemQuestionarioPorTurmaTemplatePdf relatorioSondagemQuestionarioPorTurmaTemplatePdf)
    {
        this.reportConverter = reportConverter;
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _relatorioSondagemQuestionarioPorTurmaTemplatePdf = relatorioSondagemQuestionarioPorTurmaTemplatePdf;
    }

    public async Task<string> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(RelatorioSondagemPorTurmaDto consultaSondagemPorTurmaDto)
    {
        var relatorioHtml = _relatorioSondagemQuestionarioPorTurmaTemplatePdf.GerarHtml(consultaSondagemPorTurmaDto);

        byte[] pdfBytes = reportConverter.GerarPdfEmMemoria(relatorioHtml);
        string nomeArquivo = $"Relatorio/{consultaSondagemPorTurmaDto.CodigoCorrelacao}.pdf";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(pdfBytes, nomeArquivo);

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}
