using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaExcel : IRelatorioSondagemQuestionarioPorTurmaExcel
{
    private readonly IServicoArmazenamentoMinio _servicoArmazenamentoMinio;
    private readonly IRelatorioSondagemQuestionarioPorTurmaTemplateExcel _relatorioSondagemQuestionarioPorTurmaTemplateExcel;

    public RelatorioSondagemQuestionarioPorTurmaExcel(
        IServicoArmazenamentoMinio servicoArmazenamentoMinio, IRelatorioSondagemQuestionarioPorTurmaTemplateExcel relatorioSondagemQuestionarioPorTurmaTemplateExcel)
    {
        _servicoArmazenamentoMinio = servicoArmazenamentoMinio;
        _relatorioSondagemQuestionarioPorTurmaTemplateExcel = relatorioSondagemQuestionarioPorTurmaTemplateExcel;
    }

    public async Task<string> GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto)
    {
        var stream = _relatorioSondagemQuestionarioPorTurmaTemplateExcel.GerarExcelEF(relatorioSondagemPorTurmaDto);
        return await EnviarExcelParaMinio(stream, relatorioSondagemPorTurmaDto.CodigoCorrelacao);
    }

    private async Task<string> EnviarExcelParaMinio(Stream stream, Guid codigoCorrelacao)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var excelBytes = ms.ToArray();

        string nomeArquivo = $"Relatorio/{codigoCorrelacao}.xlsx";

        await _servicoArmazenamentoMinio.UploadRelatorioAsync(
            excelBytes,
            nomeArquivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        return await _servicoArmazenamentoMinio.GerarLinkDownloadAsync(nomeArquivo);
    }
}