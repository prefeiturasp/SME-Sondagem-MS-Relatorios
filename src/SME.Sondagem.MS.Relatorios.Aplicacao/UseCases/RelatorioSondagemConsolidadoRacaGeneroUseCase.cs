using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaGeneroUseCase
    : RelatorioSondagemConsolidadoUseCaseBase,
      IRelatorioSondagemConsolidadoRacaGeneroUseCase
{
    private readonly IRelatorioSondagemConsolidadoRacaGeneroPdf _pdf;
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;
    private readonly IRepositorioRacaCor _repositorioRacaCor;
    private readonly IRepositorioGeneroSexo _repositorioGeneroSexo;

    public RelatorioSondagemConsolidadoRacaGeneroUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoRacaGeneroPdf relatorioSondagemConsolidadoRacaGeneroPdf,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IRepositorioRacaCor repositorioRacaCor,
        IRepositorioGeneroSexo repositorioGeneroSexo,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoRacaGeneroUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger,
            new ConsolidadoRepositorios(repositorioComponenteCurricular, repositorioGeneroSexo, repositorioRacaCor))
    {
        _pdf = relatorioSondagemConsolidadoRacaGeneroPdf;
        _excel = relatorioSondagemConsolidadoGenericoExcel;
        _repositorioRacaCor = repositorioRacaCor;
        _repositorioGeneroSexo = repositorioGeneroSexo;
    }

    protected override string AgrupamentoPadrao => "Por raça e gênero";

    protected override string ContextoRelatorioParaLog => "por raça e gênero";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaGeneroAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.RacasDisponiveis = await _repositorioRacaCor.ObterTodosAsync();
        dadosRelatorio.GenerosDisponiveis = await _repositorioGeneroSexo.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);


}
