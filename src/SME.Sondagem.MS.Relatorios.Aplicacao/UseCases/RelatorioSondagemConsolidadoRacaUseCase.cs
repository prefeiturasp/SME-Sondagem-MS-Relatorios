using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoRacaUseCase
{
    private readonly IRelatorioSondagemConsolidadoRacaPdf _pdf;
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;
    private readonly IRepositorioRacaCor _repositorioRacaCor;

    public RelatorioSondagemConsolidadoRacaUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoRacaPdf relatorioSondagemConsolidadoRacaPdf,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IRepositorioRacaCor repositorioRacaCor,
        IRepositorioGeneroSexo repositorioGeneroSexo,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoRacaUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular, repositorioGeneroSexo, repositorioRacaCor)
    {
        _pdf = relatorioSondagemConsolidadoRacaPdf;
        _excel = relatorioSondagemConsolidadoGenericoExcel;
        _repositorioRacaCor = repositorioRacaCor;
    }

    protected override string AgrupamentoPadrao => "Por raça";

    protected override string ContextoRelatorioParaLog => "por raça";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.RacasDisponiveis = await _repositorioRacaCor.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);


}
