using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
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
    private readonly ConsolidadoRepositorios _repositorios;

    protected RelatorioSondagemConsolidadoUseCaseBase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger logger,
        ConsolidadoRepositorios repositorios)
    {
        ServicoSondagemApiClient = servicoSondagemApiClient;
        _servicoSgpApiClient = servicoSgpApiClient;
        _servicoEolApiClient = servicoEolApiClient;
        _servicoMensageria = servicoMensageria;
        _logger = logger;
        _repositorios = repositorios;
    }

    protected abstract string AgrupamentoPadrao { get; }

    protected abstract string ContextoRelatorioParaLog { get; }

    protected abstract Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros);

    protected abstract Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio);

    protected abstract Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio);

    protected virtual void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio) { }

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
        var filtros = mensagem.FiltrosUsados;
        var dadosTask = ObterRelatorioConsolidadoAsync(filtros);
        var usuarioTask = _servicoEolApiClient.ObterDadosUsuarioAsync(mensagem.UsuarioQueSolicitou);
        var profTask = ServicoSondagemApiClient.ObterProficienciaPorIdAsync(filtros.ProficienciaId);
        var componentesCurricularTask = _repositorios.ComponenteCurricular.ObterPorIdAsync(filtros.ComponenteCurricularId);

        Task<List<EscolaDto>>? escolaTask = !string.IsNullOrWhiteSpace(filtros.Ue)
            ? _servicoEolApiClient.ObterDadosDreAsync([filtros.Ue])
            : null;

        Task<List<DreDto>>? dreTask = !string.IsNullOrWhiteSpace(filtros.Dre) && string.IsNullOrWhiteSpace(filtros.Ue)
            ? _servicoEolApiClient.ObterNomeAbreviacaoDresAsync()
            : null;

        var allTasks = new List<Task> { dadosTask, usuarioTask, profTask, componentesCurricularTask };
        if (escolaTask != null) allTasks.Add(escolaTask);
        if (dreTask != null) allTasks.Add(dreTask);
        await Task.WhenAll(allTasks);

        var dadosRelatorio = dadosTask.Result ?? new RelatorioConsolidadoSondagemDto();
        var escola = escolaTask?.Result.FirstOrDefault();
        var dre = dreTask?.Result.FirstOrDefault(d => d.Codigo == filtros.Dre);
        await PreencherCabecalho(dadosRelatorio, mensagem, usuarioTask.Result, profTask.Result, componentesCurricularTask.Result, escola, dre, codigoCorrelacao);

        return dadosRelatorio;
    }

    private async Task PreencherCabecalho(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        MensagemSondagemDto mensagem,
        DadosUsuarioDto? usuario,
        ProficienciaDto? proficiencia,
        ComponenteCurricular componenteCurricular,
        EscolaDto? escola,
        DreDto? dre,
        Guid codigoCorrelacao)
    {
        var filtros = mensagem.FiltrosUsados;
        var proficienciaNome = proficiencia?.Nome ?? string.Empty;

        AtribuirIdentificacaoCabecalho(dadosRelatorio, mensagem, filtros, codigoCorrelacao);
        AplicarFallbacksCabecalhoApartirDosFiltros(dadosRelatorio, filtros, proficienciaNome, escola, dre);
        GarantirMetadadoDemograficoPadrao(dadosRelatorio);
        await ResolverNomesGeneroRacaAsync(dadosRelatorio);
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Genero))
            dadosRelatorio.Genero = ValorTodos;
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Raca))
            dadosRelatorio.Raca = ValorTodas;
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
        dadosRelatorio.ModalidadeId = filtros.Modalidade;
        dadosRelatorio.SemestreId = filtros.Modalidade == (int)Modalidade.EJA
            ? (filtros.BimestreId ?? filtros.SemestreId)
            : filtros.SemestreId;
        dadosRelatorio.GeneroId = filtros.GeneroId;
        dadosRelatorio.RacaId = filtros.RacaId;
        dadosRelatorio.Pap = filtros.Pap;
        dadosRelatorio.Aee = filtros.Aee;
        dadosRelatorio.Deficiente = filtros.Deficiente;
        dadosRelatorio.PossuiLinguaPortuguesaSegundaLingua = filtros.PossuiLinguaPortuguesaSegundaLingua;
    }

    private void AplicarFallbacksCabecalhoApartirDosFiltros(
        RelatorioConsolidadoSondagemDto dadosRelatorio,
        FiltroRelatorioSondagemDto filtros,
        string proficienciaNome,
        EscolaDto? escola,
        DreDto? dre = null)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Agrupamento))
            dadosRelatorio.Agrupamento = AgrupamentoPadrao;

        if (dadosRelatorio.AnoLetivo == 0)
            dadosRelatorio.AnoLetivo = filtros.AnoLetivo;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Modalidade))
            dadosRelatorio.Modalidade = ((Modalidade)filtros.Modalidade).ShortName() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Dre))
            dadosRelatorio.Dre = ResolverDre(filtros, escola, dre);

        if (string.IsNullOrWhiteSpace(dadosRelatorio.UnidadeEducacional))
            dadosRelatorio.UnidadeEducacional = ResolverUnidadeEducacional(filtros, escola);

        if (string.IsNullOrWhiteSpace(dadosRelatorio.AnoTurma))
            dadosRelatorio.AnoTurma = FormatarAnoTurma(ResolverAnos(filtros));

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Proficiencia))
            dadosRelatorio.Proficiencia = proficienciaNome;

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Bimestre))
            dadosRelatorio.Bimestre = DescricaoBimestre(filtros.BimestreId);
    }

    private static string ResolverDre(FiltroRelatorioSondagemDto filtros, EscolaDto? escola, DreDto? dre = null)
    {
        if (escola != null && !string.IsNullOrWhiteSpace(escola.NomeDRE))
            return escola.NomeDRE;
        if (dre != null && !string.IsNullOrWhiteSpace(dre.Nome))
            return dre.Nome;
        if (!string.IsNullOrWhiteSpace(filtros.Dre))
            return filtros.Dre;
        return ValorTodas;
    }

    private static string ResolverUnidadeEducacional(FiltroRelatorioSondagemDto filtros, EscolaDto? escola)
    {
        if (escola != null)
            return $"{escola.SiglaTipoEscola} - {escola.NomeEscola}";
        if (!string.IsNullOrWhiteSpace(filtros.Ue))
            return filtros.Ue;
        if (!string.IsNullOrWhiteSpace(filtros.UeCodigo))
            return filtros.UeCodigo;
        return ValorTodas;
    }

    private static List<int> ResolverAnos(FiltroRelatorioSondagemDto filtros)
    {
        if (filtros.AnoTurma.Count > 0)
            return filtros.AnoTurma;
        if (filtros.Ano > 0)
            return [filtros.Ano];
        return [];
    }

    private async Task ResolverNomesGeneroRacaAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Genero) && dadosRelatorio.GeneroId.HasValue)
        {
            var generos = await _repositorios.GeneroSexo.ObterTodosAsync();
            dadosRelatorio.Genero = generos.FirstOrDefault(g => g.Id == dadosRelatorio.GeneroId)?.Descricao ?? ValorTodos;
        }

        if (string.IsNullOrWhiteSpace(dadosRelatorio.Raca) && dadosRelatorio.RacaId.HasValue)
        {
            var racas = await _repositorios.RacaCor.ObterTodosAsync();
            var racaDescricao = racas.FirstOrDefault(r => r.Id == dadosRelatorio.RacaId)?.Descricao;
            dadosRelatorio.Raca = racaDescricao != null
                ? System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(racaDescricao.ToLower())
                : ValorTodas;
        }
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

    protected static string FormatarAnoTurma(List<int> anos)
    {
        if (anos == null || anos.Count == 0)
            return ValorTodos;

        var partes = anos.Order().Select(a => $"{a}°").ToList();

        if (partes.Count == 1)
            return $"{partes[0]} ANO";

        return string.Join(", ", partes[..^1]) + $" e {partes[^1]} ANO";
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
