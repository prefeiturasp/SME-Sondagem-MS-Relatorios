using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoGeneroUseCase
    : RelatorioSondagemConsolidadoUseCaseBase,
      IRelatorioSondagemConsolidadoGeneroUseCase
{
    private readonly IRelatorioSondagemConsolidadoGeneroPdf _pdf;
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;
    private readonly IRepositorioGeneroSexo _repositorioGeneroSexo;

    public RelatorioSondagemConsolidadoGeneroUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoGeneroPdf relatorioSondagemConsolidadoGeneroPdf,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IRepositorioGeneroSexo repositorioGeneroSexo,
        IRepositorioRacaCor repositorioRacaCor,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoGeneroUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger,
            new ConsolidadoRepositorios(repositorioComponenteCurricular, repositorioGeneroSexo, repositorioRacaCor))
    {
        _pdf = relatorioSondagemConsolidadoGeneroPdf;
        _excel = relatorioSondagemConsolidadoGenericoExcel;
        _repositorioGeneroSexo = repositorioGeneroSexo;
    }

    protected override string AgrupamentoPadrao => "Por gênero";

    protected override string ContextoRelatorioParaLog => "por gênero";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorGeneroAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.GenerosDisponiveis = await _repositorioGeneroSexo.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);


}
