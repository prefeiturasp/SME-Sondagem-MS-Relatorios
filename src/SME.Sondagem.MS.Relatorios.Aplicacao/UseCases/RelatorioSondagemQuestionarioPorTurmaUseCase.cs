using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Infra.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Mappers;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemQuestionarioPorTurmaUseCase : IRelatorioSondagemQuestionarioPorTurmaUseCase
{
    private readonly IServicoSondagemApiClient _servicoSondagemApiClient;
    private readonly IRelatorioSondagemQuestionarioPorTurmaPdf _relatorioSondagemQuestionarioPorTurmaPdf;
    private readonly IRelatorioSondagemQuestionarioPorTurmaExcel _relatorioSondagemQuestionarioPorTurmaExcel;
    private readonly IServicoSgpApiClient _servicoSgpApiClient;
    private readonly ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> _logger;

    public RelatorioSondagemQuestionarioPorTurmaUseCase(IServicoSondagemApiClient servicoSondagemApiClient, ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> logger, IServicoSgpApiClient servicoSgpApiClient, IRelatorioSondagemQuestionarioPorTurmaPdf relatorioSondagemQuestionarioPorTurmaPdf, IRelatorioSondagemQuestionarioPorTurmaExcel relatorioSondagemQuestionarioPorTurmaExcel)
    {
        _servicoSondagemApiClient = servicoSondagemApiClient;
        _logger = logger;
        _servicoSgpApiClient = servicoSgpApiClient;
        _relatorioSondagemQuestionarioPorTurmaPdf = relatorioSondagemQuestionarioPorTurmaPdf;
        _relatorioSondagemQuestionarioPorTurmaExcel = relatorioSondagemQuestionarioPorTurmaExcel;
    }

    public async Task<bool> Executar(MensagemRabbit param)
    {
        var filtrosRelatorio = param.ObterObjetoMensagem<MensagemSondagemQuestionarioDto>();
        if (filtrosRelatorio == null)
            return true;

        var dadosRelatorio = await _servicoSondagemApiClient.ObterDadosQuestionarioAsync(filtrosRelatorio.FiltrosUsados);
        if (dadosRelatorio == null)
            return true;

        var dto = dadosRelatorio.ParaDto();

        switch (filtrosRelatorio.ExtensaoRelatorio)
        {
            case (int)ExtensaoRelatorio.Pdf:
                await _relatorioSondagemQuestionarioPorTurmaPdf.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(dadosRelatorio);
                break;
            case (int)ExtensaoRelatorio.Xlsx:
                await _relatorioSondagemQuestionarioPorTurmaExcel.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(dto);
                break;
            default:
                break;
        }

        //await _servicoSgpApiClient.FinalizarSolicitacaoRelatorioAsync(23);
        return await Task.FromResult(true);
    }
}
