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
    private readonly IServicoEolApiClient _servicoEolApiClient;
    private readonly IServicoMensageria servicoMensageria;
    private readonly ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> _logger;

    public RelatorioSondagemQuestionarioPorTurmaUseCase(IServicoSondagemApiClient servicoSondagemApiClient, ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase> logger, IServicoSgpApiClient servicoSgpApiClient, IRelatorioSondagemQuestionarioPorTurmaPdf relatorioSondagemQuestionarioPorTurmaPdf, IRelatorioSondagemQuestionarioPorTurmaExcel relatorioSondagemQuestionarioPorTurmaExcel, IServicoMensageria servicoMensageria, IServicoEolApiClient servicoEolApiClient)
    {
        _servicoSondagemApiClient = servicoSondagemApiClient;
        _logger = logger;
        _servicoSgpApiClient = servicoSgpApiClient;
        _relatorioSondagemQuestionarioPorTurmaPdf = relatorioSondagemQuestionarioPorTurmaPdf;
        _relatorioSondagemQuestionarioPorTurmaExcel = relatorioSondagemQuestionarioPorTurmaExcel;
        this.servicoMensageria = servicoMensageria;
        _servicoEolApiClient = servicoEolApiClient;
    }

    public async Task<bool> Executar(MensagemRabbit mensagemRabbit)
    {
        var filtrosRelatorio = mensagemRabbit.ObterObjetoMensagem<MensagemSondagemPorTurmaDto>();
        
        if (filtrosRelatorio == null)
            return false;

        var linkRelatorio = string.Empty;
        try
        {
            var dadosRelatorio = await ObterDadosRelatorio(filtrosRelatorio, mensagemRabbit.CodigoCorrelacao);

            switch (filtrosRelatorio.FiltrosUsados.ExtensaoRelatorio)
            {
                case (int)ExtensaoRelatorio.Pdf:
                    linkRelatorio = await _relatorioSondagemQuestionarioPorTurmaPdf.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(dadosRelatorio);
                    break;
                case (int)ExtensaoRelatorio.Xlsx:
                    linkRelatorio = await _relatorioSondagemQuestionarioPorTurmaExcel.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(dadosRelatorio);
                    break;
                default:
                    break;
            }

            await FinalizarRelatorio(dadosRelatorio, filtrosRelatorio.SolicitacaoRelatorioId, linkRelatorio, mensagemRabbit.CodigoCorrelacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERRO AO GERAR O RELATORIO");
            return false;
        }
        
        return true;
    }

    private async Task<RelatorioSondagemPorTurmaDto> ObterDadosRelatorio(MensagemSondagemPorTurmaDto mensagemSondagemQuestionarioDto, Guid codigoCorrelacao)
    {
        var tarefaDadosRelatorio = _servicoSondagemApiClient.ObterDadosQuestionarioAsync(mensagemSondagemQuestionarioDto.FiltrosUsados);

        var tarefaDreUe = _servicoEolApiClient.ObterDadosDreAsync([mensagemSondagemQuestionarioDto.FiltrosUsados.UeCodigo]);
        var tarefaTurma = _servicoEolApiClient.ObterDadosTurmaAsync(mensagemSondagemQuestionarioDto.FiltrosUsados.TurmaId);
        var tarefaUsuario = _servicoEolApiClient.ObterDadosUsuarioAsync(mensagemSondagemQuestionarioDto.UsuarioQueSolicitou);

        await Task.WhenAll(tarefaDadosRelatorio, tarefaDreUe, tarefaTurma, tarefaUsuario);

        var dadosRelatorio = tarefaDadosRelatorio.Result;
        var dreUe = tarefaDreUe.Result;
        var turma = tarefaTurma.Result;
        var usuario = tarefaUsuario.Result;
        var parametroSondagem = await _servicoSondagemApiClient.ObterParametrosSondagemPorQuestionarioId(dadosRelatorio.QuestionarioId);

        usuario.CodigoRf = mensagemSondagemQuestionarioDto.UsuarioQueSolicitou;

        if (mensagemSondagemQuestionarioDto == null)
            return new RelatorioSondagemPorTurmaDto();

        if (dadosRelatorio == null)
            return new RelatorioSondagemPorTurmaDto();

        var escola = dreUe.FirstOrDefault();
        var parametroPossuiLinguaPortuguesaSegundaLingua =  parametroSondagem?.Where(x => x.Tipo == "PossuiLinguaPortuguesaSegundaLingua")?.FirstOrDefault()?.Valor;
        var exibeColunaLinguaPortuguesaSegundaLingua = parametroPossuiLinguaPortuguesaSegundaLingua == "true";

        var relatorioSondagemPorTurmaDto = dadosRelatorio.ParaDto(escola, turma, usuario, 
                                        (Modalidade)mensagemSondagemQuestionarioDto.FiltrosUsados.Modalidade, 
                                        exibeColunaLinguaPortuguesaSegundaLingua);

        relatorioSondagemPorTurmaDto.CodigoCorrelacao = codigoCorrelacao;
        return relatorioSondagemPorTurmaDto;
    }

    private async Task FinalizarRelatorio(RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto, int solicitacaoRelatorioId, string linkRelatorio, Guid codigoCorrelacao)
    {
        await _servicoSgpApiClient.FinalizarSolicitacaoRelatorioAsync(new FinalizarSolicitacaoRelatorioDto(solicitacaoRelatorioId, linkRelatorio, codigoCorrelacao));
        await NotificarUsuario(relatorioSondagemPorTurmaDto, codigoCorrelacao);
    }

    private async Task NotificarUsuario(RelatorioSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao)
    {
        var menssagemUsuario = $"Relatório da Sondagem de escrita da turma {consultaSondagemPorTurmaDto.Turma} da {consultaSondagemPorTurmaDto.UnidadeEducacional} ({consultaSondagemPorTurmaDto.SiglaDre})";
        var mensagemRelatorioPronto = new MensagemRelatorioProntoDto(menssagemUsuario, "", "");

        var mensagem = new MensagemRabbit(mensagemRelatorioPronto, codigoCorrelacao);
        await servicoMensageria.Publicar(mensagem, RotasRabbit.RotaRelatoriosProntosSgp, ExchangeRabbit.Sgp);
    }
}
