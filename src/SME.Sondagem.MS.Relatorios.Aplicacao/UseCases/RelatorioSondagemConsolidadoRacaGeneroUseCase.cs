using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaGeneroUseCase
    : RelatorioSondagemConsolidadoUseCaseBase,
      IRelatorioSondagemConsolidadoRacaGeneroUseCase
{
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;

    public RelatorioSondagemConsolidadoRacaGeneroUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoRacaGeneroUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular)
    {
        _excel = relatorioSondagemConsolidadoGenericoExcel;
    }

    protected override string AgrupamentoPadrao => "Por raça e gênero";

    protected override string ContextoRelatorioParaLog => "por raça e gênero";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemGenericoDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaGeneroAsync(filtros);

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        throw new NotImplementedException();

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
    }
}
