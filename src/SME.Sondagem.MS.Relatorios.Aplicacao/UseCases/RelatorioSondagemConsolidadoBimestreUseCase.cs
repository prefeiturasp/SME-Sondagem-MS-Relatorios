using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoBimestreUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoBimestreUseCase
{
    private readonly IRelatorioSondagemConsolidadoBimestrePdf _pdf;
    private readonly IRepositorioBimestre _repositorioBimestre;

    public RelatorioSondagemConsolidadoBimestreUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoBimestrePdf relatorioSondagemConsolidadoBimestrePdf,
        IRepositorioBimestre repositorioBimestre,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoBimestreUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular)
    {
        _pdf = relatorioSondagemConsolidadoBimestrePdf;
        _repositorioBimestre = repositorioBimestre;
    }

    protected override string AgrupamentoPadrao => "Por bimestre";

    protected override string ContextoRelatorioParaLog => "por bimestre";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorBimestreAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.BimestresDisponiveis = await _repositorioBimestre.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) => throw new NotImplementedException();

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
    }
}
