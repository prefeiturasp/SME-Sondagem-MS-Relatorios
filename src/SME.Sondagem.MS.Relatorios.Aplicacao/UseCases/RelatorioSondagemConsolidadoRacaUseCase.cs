using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaUseCase : IRelatorioSondagemConsolidadoRacaUseCase
{
    private const string AGRUPAMENTO_POR_RACA = "Por raça";
    private const string VALOR_TODAS = "Todas";
    private const string VALOR_TODOS = "Todos";

    private readonly IServicoSondagemApiClient _servicoSondagemApiClient;
    private readonly IRelatorioSondagemConsolidadoRacaPdf _relatorioSondagemConsolidadoRacaPdf;
    private readonly IServicoSgpApiClient _servicoSgpApiClient;
    private readonly IServicoEolApiClient _servicoEolApiClient;
    private readonly IServicoMensageria _servicoMensageria;
    private readonly ILogger<RelatorioSondagemConsolidadoRacaUseCase> _logger;

    public RelatorioSondagemConsolidadoRacaUseCase(IServicoSondagemApiClient servicoSondagemApiClient,
                                                   IRelatorioSondagemConsolidadoRacaPdf relatorioSondagemConsolidadoRacaPdf,
                                                   IServicoSgpApiClient servicoSgpApiClient,
                                                   IServicoEolApiClient servicoEolApiClient,
                                                   IServicoMensageria servicoMensageria,
                                                   ILogger<RelatorioSondagemConsolidadoRacaUseCase> logger)
    {
        _servicoSondagemApiClient = servicoSondagemApiClient;
        _relatorioSondagemConsolidadoRacaPdf = relatorioSondagemConsolidadoRacaPdf;
        _servicoSgpApiClient = servicoSgpApiClient;
        _servicoEolApiClient = servicoEolApiClient;
        _servicoMensageria = servicoMensageria;
        _logger = logger;
    }

    public async Task<bool> Executar(MensagemRabbit mensagemRabbit)
    {
        var filtrosRelatorio = mensagemRabbit.ObterObjetoMensagem<MensagemSondagemPorTurmaDto>();

        if (filtrosRelatorio == null)
            return false;

        try
        {
            var dadosRelatorio = await ObterDadosRelatorio(filtrosRelatorio, mensagemRabbit.CodigoCorrelacao);

            var linkRelatorio = await _relatorioSondagemConsolidadoRacaPdf.Executar(dadosRelatorio);

            await FinalizarRelatorio(dadosRelatorio, filtrosRelatorio.SolicitacaoRelatorioId, linkRelatorio, mensagemRabbit.CodigoCorrelacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERRO AO GERAR O RELATORIO CONSOLIDADO POR RAÇA");
            return false;
        }

        return true;
    }

    private async Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorio(MensagemSondagemPorTurmaDto mensagem, Guid codigoCorrelacao)
    {
        var tarefaDadosRelatorio = _servicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaAsync(mensagem.FiltrosUsados);
        var tarefaUsuario = _servicoEolApiClient.ObterDadosUsuarioAsync(mensagem.UsuarioQueSolicitou);
        var tarefaProficiencia = _servicoSondagemApiClient.ObterProficienciaPorIdAsync(mensagem.FiltrosUsados.ProficienciaId);

        await Task.WhenAll(tarefaDadosRelatorio, tarefaUsuario, tarefaProficiencia);

        var dadosRelatorio = tarefaDadosRelatorio.Result ?? new RelatorioConsolidadoSondagemDto();
        var usuario = tarefaUsuario.Result;
        var proficiencia = tarefaProficiencia.Result;

        PreencherCabecalho(dadosRelatorio, mensagem, usuario, proficiencia, codigoCorrelacao);

        return dadosRelatorio;
    }

    private static void PreencherCabecalho(RelatorioConsolidadoSondagemDto dadosRelatorio,
                                           MensagemSondagemPorTurmaDto mensagem,
                                           DadosUsuarioDto? usuario,
                                           ProficienciaDto? proficiencia,
                                           Guid codigoCorrelacao)
    {
        var filtros = mensagem.FiltrosUsados;

        dadosRelatorio.CodigoCorrelacao = codigoCorrelacao;
        dadosRelatorio.SolicitacaoRelatorioId = mensagem.SolicitacaoRelatorioId;
        dadosRelatorio.UsuarioQueSolicitou = mensagem.UsuarioQueSolicitou;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Agrupamento))
            dadosRelatorio.Agrupamento = AGRUPAMENTO_POR_RACA;

        if (dadosRelatorio.AnoLetivo == 0)
            dadosRelatorio.AnoLetivo = filtros.AnoLetivo;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Modalidade))
            dadosRelatorio.Modalidade = ((Modalidade)filtros.Modalidade).ShortName() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Dre))
            dadosRelatorio.Dre = VALOR_TODAS;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.UnidadeEducacional))
            dadosRelatorio.UnidadeEducacional = string.IsNullOrWhiteSpace(filtros.UeCodigo) ? VALOR_TODAS : filtros.UeCodigo;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.AnoTurma))
            dadosRelatorio.AnoTurma = filtros.Ano > 0 ? $"{filtros.Ano}° ANO" : VALOR_TODOS;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Proficiencia))
            dadosRelatorio.Proficiencia = proficiencia?.Nome ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Bimestre))
            dadosRelatorio.Bimestre = ObterDescricaoBimestre(filtros.BimestreId);

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Raca))
            dadosRelatorio.Raca = VALOR_TODAS;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Usuario) && usuario != null)
            dadosRelatorio.Usuario = $"{usuario.Nome} ({mensagem.UsuarioQueSolicitou})";

        if (dadosRelatorio.DataImpressao == default)
            dadosRelatorio.DataImpressao = DateTime.Now;
    }

    private static string ObterDescricaoBimestre(int? bimestreId)
    {
        if (bimestreId is null or 0)
            return VALOR_TODOS;

        return Enum.TryParse(bimestreId.ToString(), out Bimestre bimestre)
            ? bimestre.ShortName() ?? VALOR_TODOS
            : VALOR_TODOS;
    }

    private async Task FinalizarRelatorio(RelatorioConsolidadoSondagemDto dadosRelatorio, int solicitacaoRelatorioId, string linkRelatorio, Guid codigoCorrelacao)
    {
        await _servicoSgpApiClient.FinalizarSolicitacaoRelatorioAsync(
            new FinalizarSolicitacaoRelatorioDto(solicitacaoRelatorioId, linkRelatorio, codigoCorrelacao));

        await NotificarUsuario(dadosRelatorio, codigoCorrelacao);
    }

    private async Task NotificarUsuario(RelatorioConsolidadoSondagemDto dadosRelatorio, Guid codigoCorrelacao)
    {
        var mensagemUsuario = $"Relatório da Sondagem - {dadosRelatorio.Titulo} - Agrupamento: {dadosRelatorio.Agrupamento}";
        var mensagemRelatorioPronto = new MensagemRelatorioProntoDto(mensagemUsuario, "", "");

        var mensagem = new MensagemRabbit(mensagemRelatorioPronto, codigoCorrelacao);
        await _servicoMensageria.Publicar(mensagem, RotasRabbit.RotaRelatoriosProntosSgp, ExchangeRabbit.Sgp);
    }
}
