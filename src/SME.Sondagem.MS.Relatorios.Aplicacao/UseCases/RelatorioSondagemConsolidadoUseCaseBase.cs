using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public abstract class RelatorioSondagemConsolidadoUseCaseBase
{
    protected const string ValorTodas = "Todas";
    protected const string ValorTodos = "Todos";

    protected readonly IServicoSondagemApiClient ServicoSondagemApiClient;
    private readonly IServicoSgpApiClient _servicoSgpApiClient;
    private readonly IServicoEolApiClient _servicoEolApiClient;
    private readonly IServicoMensageria _servicoMensageria;
    private readonly ILogger _logger;
    private readonly IRepositorioComponenteCurricular _repositorioComponenteCurricular;

    protected RelatorioSondagemConsolidadoUseCaseBase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
    {
        ServicoSondagemApiClient = servicoSondagemApiClient;
        _servicoSgpApiClient = servicoSgpApiClient;
        _servicoEolApiClient = servicoEolApiClient;
        _servicoMensageria = servicoMensageria;
        _logger = logger;
        _repositorioComponenteCurricular = repositorioComponenteCurricular;
    }

    protected abstract string AgrupamentoPadrao { get; }

    protected abstract string ContextoRelatorioParaLog { get; }

    protected abstract Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros);

    protected abstract Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio);

    protected abstract Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio);

    protected abstract void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio);

    public async Task<bool> Executar(MensagemRabbit mensagemRabbit)
    {
        var mensagem = mensagemRabbit.ObterObjetoMensagem<MensagemSondagemDto>();
        if (mensagem == null)
            return false;

        try
        {
            var linkRelatorio = string.Empty;

            var dadosRelatorio = await MontarDadosRelatorioAsync(mensagem, mensagemRabbit.CodigoCorrelacao);

            switch (mensagem.FiltrosUsados.ExtensaoRelatorio)
            {
                case (int)ExtensaoRelatorio.Pdf:
                    linkRelatorio = await GerarPdfAsync(dadosRelatorio);
                    break;
                case (int)ExtensaoRelatorio.Xlsx:
                    linkRelatorio = await GerarExcelAsync(dadosRelatorio);
                    break;
                default:
                    break;
            }

            await FinalizarRelatorioAsync(dadosRelatorio, mensagem.SolicitacaoRelatorioId, linkRelatorio, mensagemRabbit.CodigoCorrelacao);
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex, "Erro ao gerar relatório consolidado {Contexto}", ContextoRelatorioParaLog);
            return false;
        }

        return true;
    }

    private async Task<RelatorioConsolidadoSondagemDto> MontarDadosRelatorioAsync(MensagemSondagemDto mensagem, Guid codigoCorrelacao)
    {
        var dadosTask = ObterRelatorioConsolidadoAsync(mensagem.FiltrosUsados);
        var usuarioTask = _servicoEolApiClient.ObterDadosUsuarioAsync(mensagem.UsuarioQueSolicitou);
        var profTask = ServicoSondagemApiClient.ObterProficienciaPorIdAsync(mensagem.FiltrosUsados.ProficienciaId);
        var componentesCurricularTask = _repositorioComponenteCurricular.ObterPorIdAsync(mensagem.FiltrosUsados.ComponenteCurricularId);

        await Task.WhenAll(dadosTask, usuarioTask, profTask, componentesCurricularTask);

        var dadosRelatorio = await dadosTask ?? new RelatorioConsolidadoSondagemDto();
        PreencherCabecalho(dadosRelatorio, mensagem, await usuarioTask, await profTask, await componentesCurricularTask, codigoCorrelacao);

        return dadosRelatorio;
    }

    private void PreencherCabecalho(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        MensagemSondagemDto mensagem,
        DadosUsuarioDto? usuario,
        ProficienciaDto? proficiencia,
        ComponenteCurricular componenteCurricular,
        Guid codigoCorrelacao)
    {
        var filtros = mensagem.FiltrosUsados;
        var proficienciaNome = proficiencia?.Nome ?? string.Empty;

        AtribuirIdentificacaoCabecalho(dadosRelatorio, mensagem, filtros, codigoCorrelacao);
        AplicarFallbacksCabecalhoApartirDosFiltros(dadosRelatorio, filtros, proficienciaNome);
        GarantirMetadadoDemograficoPadrao(dadosRelatorio);
        FinalizarCabecalhoUsuarioTituloComponente(dadosRelatorio, mensagem, usuario, proficienciaNome, componenteCurricular);
    }

    private static void AtribuirIdentificacaoCabecalho(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        MensagemSondagemDto mensagem,
        FiltroRelatorioSondagemDto filtros,
        Guid codigoCorrelacao)
    {
        dadosRelatorio.CodigoCorrelacao = codigoCorrelacao;
        dadosRelatorio.SolicitacaoRelatorioId = mensagem.SolicitacaoRelatorioId;
        dadosRelatorio.UsuarioQueSolicitou = mensagem.UsuarioQueSolicitou;
        dadosRelatorio.ProficienciaId = filtros.ProficienciaId;
    }

    private void AplicarFallbacksCabecalhoApartirDosFiltros(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        FiltroRelatorioSondagemDto filtros,
        string proficienciaNome)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Agrupamento))
            dadosRelatorio.Agrupamento = AgrupamentoPadrao;

        if (dadosRelatorio.AnoLetivo == 0)
            dadosRelatorio.AnoLetivo = filtros.AnoLetivo;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Modalidade))
            dadosRelatorio.Modalidade = ((Modalidade)filtros.Modalidade).ShortName() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Dre))
            dadosRelatorio.Dre = ValorTodas;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.UnidadeEducacional))
            dadosRelatorio.UnidadeEducacional = string.IsNullOrWhiteSpace(filtros.UeCodigo) ? ValorTodas : filtros.UeCodigo;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.AnoTurma))
            dadosRelatorio.AnoTurma = filtros.Ano > 0 ? $"{filtros.Ano}° ANO" : ValorTodos;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Proficiencia))
            dadosRelatorio.Proficiencia = proficienciaNome;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Bimestre))
            dadosRelatorio.Bimestre = DescricaoBimestre(filtros.BimestreId);
    }

    private static void FinalizarCabecalhoUsuarioTituloComponente(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        MensagemSondagemDto mensagem,
        DadosUsuarioDto? usuario,
        string proficienciaNome,
        ComponenteCurricular componenteCurricular)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Usuario) && usuario != null)
            dadosRelatorio.Usuario = $"{usuario.Nome} ({mensagem.UsuarioQueSolicitou})";

        if (dadosRelatorio.DataImpressao == default)
            dadosRelatorio.DataImpressao = DateTime.Now;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.ComponenteCurricular))
            dadosRelatorio.ComponenteCurricular = componenteCurricular.Nome ?? string.Empty;

        dadosRelatorio.Titulo = $"{proficienciaNome} consolidado";
    }

    protected static string DescricaoBimestre(int? bimestreId)
    {
        if (bimestreId is null or 0)
            return ValorTodos;

        return Enum.TryParse(bimestreId.ToString(), out Dominio.Enums.Bimestre bimestre)
            ? bimestre.ShortName() ?? ValorTodos
            : ValorTodos;
    }

    private async Task FinalizarRelatorioAsync(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        int solicitacaoRelatorioId,
        string linkRelatorio,
        Guid codigoCorrelacao)
    {
        await _servicoSgpApiClient.FinalizarSolicitacaoRelatorioAsync(
            new FinalizarSolicitacaoRelatorioDto(solicitacaoRelatorioId, linkRelatorio, codigoCorrelacao));

        await NotificarUsuarioAsync(dadosRelatorio, codigoCorrelacao);
    }

    private async Task NotificarUsuarioAsync(RelatorioConsolidadoSondagemDto dadosRelatorio, Guid codigoCorrelacao)
    {
        var texto = $"Relatório da Sondagem - {dadosRelatorio.Titulo} - Agrupamento: {dadosRelatorio.Agrupamento}";
        var payload = new MensagemRelatorioProntoDto(texto, "", "");
        await _servicoMensageria.Publicar(new MensagemRabbit(payload, codigoCorrelacao), RotasRabbit.RotaRelatoriosProntosSgp, ExchangeRabbit.Sgp);
    }
}
