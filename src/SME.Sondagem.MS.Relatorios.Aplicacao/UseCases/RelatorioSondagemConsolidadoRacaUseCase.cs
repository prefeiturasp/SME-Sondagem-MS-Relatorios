using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaUseCase : RelatorioSondagemConsolidadoUseCaseBase<RelatorioSondagemConsolidadoRacaUseCase>, IRelatorioSondagemConsolidadoRacaUseCase
{
    private readonly IRelatorioSondagemConsolidadoRacaPdf _pdf;
    private readonly IRepositorioRacaCor _repositorioRacaCor;

    public RelatorioSondagemConsolidadoRacaUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoRacaPdf relatorioSondagemConsolidadoRacaPdf,
        IRepositorioRacaCor repositorioRacaCor,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoRacaUseCase> logger)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger)
    {
        _pdf = relatorioSondagemConsolidadoRacaPdf;
        _repositorioRacaCor = repositorioRacaCor;
    }

    protected override string AgrupamentoPadrao => "Por raça";

    protected override string ContextoRelatorioParaLog => "por raça";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.RacasDisponiveis = await _repositorioRacaCor.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Raca))
            dadosRelatorio.Raca = ValorTodas;
    }
}
