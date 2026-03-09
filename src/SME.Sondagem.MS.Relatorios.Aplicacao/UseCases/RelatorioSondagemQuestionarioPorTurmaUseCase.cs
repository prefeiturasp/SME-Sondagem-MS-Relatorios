using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
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
    private readonly IServicoMensageria servicoMensageria;
    private readonly ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> _logger;

    public RelatorioSondagemQuestionarioPorTurmaUseCase(IServicoSondagemApiClient servicoSondagemApiClient, ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> logger, IServicoSgpApiClient servicoSgpApiClient, IRelatorioSondagemQuestionarioPorTurmaPdf relatorioSondagemQuestionarioPorTurmaPdf, IRelatorioSondagemQuestionarioPorTurmaExcel relatorioSondagemQuestionarioPorTurmaExcel, IServicoMensageria servicoMensageria)
    {
        _servicoSondagemApiClient = servicoSondagemApiClient;
        _logger = logger;
        _servicoSgpApiClient = servicoSgpApiClient;
        _relatorioSondagemQuestionarioPorTurmaPdf = relatorioSondagemQuestionarioPorTurmaPdf;
        _relatorioSondagemQuestionarioPorTurmaExcel = relatorioSondagemQuestionarioPorTurmaExcel;
        this.servicoMensageria = servicoMensageria;
    }

    public async Task<bool> Executar(MensagemRabbit mensagemRabbit)
    {
        var filtrosRelatorio = mensagemRabbit.ObterObjetoMensagem<MensagemSondagemQuestionarioDto>();

        try
        {
            if (filtrosRelatorio == null)
                return true;

            var dadosRelatorio = await _servicoSondagemApiClient.ObterDadosQuestionarioAsync(filtrosRelatorio.FiltrosUsados);
            if (dadosRelatorio == null)
                return true;

            var dto = dadosRelatorio.ParaDto();

            switch (filtrosRelatorio.ExtensaoRelatorio)
            {
                case (int)ExtensaoRelatorio.Pdf:
                    await _relatorioSondagemQuestionarioPorTurmaPdf.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(dto, mensagemRabbit.CodigoCorrelacao);
                    break;
                case (int)ExtensaoRelatorio.Xlsx:
                    await _relatorioSondagemQuestionarioPorTurmaExcel.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(dto);
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem no RabbitMQ");
            await servicoMensageria.Publicar(new MensagemRabbit("Erro ao gerar relatório", mensagemRabbit.CodigoCorrelacao), RotasRabbit.RotaRelatorioComErro, ExchangeRabbit.WorkerRelatorios);
            return false;
        }

        await NotificarUsuario(mensagemRabbit);

        await _servicoSgpApiClient.FinalizarSolicitacaoRelatorioAsync(filtrosRelatorio.SolicitacaoRelatorioId);
        return await Task.FromResult(true);
    }

    private async Task NotificarUsuario(MensagemRabbit mensagemRabbit)
    {
        var mensagem = new MensagemRabbit("Relatório gerado com sucesso", mensagemRabbit.CodigoCorrelacao);
        await servicoMensageria.Publicar(mensagem, RotasRabbit.RotaRelatoriosProntosSgp, ExchangeRabbit.WorkerRelatorios);
    }
}
