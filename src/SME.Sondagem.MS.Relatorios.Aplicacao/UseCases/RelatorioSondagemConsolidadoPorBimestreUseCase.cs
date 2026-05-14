using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoPorBimestreUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoPorBimestreUseCase
{
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;
    private readonly IRelatorioSondagemConsolidadoBimestrePdf _pdf;
    private readonly IRepositorioBimestre _repositorioBimestre;

    public RelatorioSondagemConsolidadoPorBimestreUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoPorBimestreUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular,
        IRepositorioBimestre repositorioBimestre,
        IRelatorioSondagemConsolidadoBimestrePdf pdf,
        IRepositorioGeneroSexo repositorioGeneroSexo,
        IRepositorioRacaCor repositorioRacaCor)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular, repositorioGeneroSexo, repositorioRacaCor)
    {
        _excel = relatorioSondagemConsolidadoGenericoExcel;
        _repositorioBimestre = repositorioBimestre;
        _pdf = pdf;
    }

    protected override string AgrupamentoPadrao => "Por bimestre";

    protected override string ContextoRelatorioParaLog => "por bimestre";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorBimestreAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.BimestresDisponiveis = await _repositorioBimestre.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Bimestre))
            dadosRelatorio.Bimestre = ValorTodos;
    }
}
