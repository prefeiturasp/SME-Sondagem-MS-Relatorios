using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemQuestionarioPorTurmaUseCase : IRelatorioSondagemQuestionarioPorTurmaUseCase
{
    private readonly IServicoSondagemApiClient _servicoSondagemApiClient;
    private readonly ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> _logger;

    public RelatorioSondagemQuestionarioPorTurmaUseCase(IServicoSondagemApiClient servicoSondagemApiClient, ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> logger)
    {
        _servicoSondagemApiClient = servicoSondagemApiClient;
        _logger = logger;
    }

    public async Task<bool> Executar(MensagemRabbit param)
    {
        var mensagem = param.ObterObjetoMensagem<MensagemSondagemQuestionarioDto>();

        var tarefaApi = await _servicoSondagemApiClient.ObterDadosQuestionarioAsync(mensagem.FiltrosUsados);

        return await Task.FromResult(true);
    }
}
