using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoAnoUseCase
    : RelatorioSondagemConsolidadoUseCaseBase,
      IRelatorioSondagemConsolidadoAnoUseCase
{
    public RelatorioSondagemConsolidadoAnoUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoAnoUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular)
    {
    }

    protected override string AgrupamentoPadrao => "Por ano";

    protected override string ContextoRelatorioParaLog => "por ano";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        throw new NotImplementedException();

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        throw new NotImplementedException();

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        throw new NotImplementedException();

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
    }
}
