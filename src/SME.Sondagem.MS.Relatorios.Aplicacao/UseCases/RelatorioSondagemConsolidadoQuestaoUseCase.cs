using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoQuestaoUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoQuestaoUseCase
{
    private readonly IRelatorioSondagemConsolidadoQuestaoPdf _pdf;
    private readonly IRelatorioSondagemConsolidadoGenericoExcel _excel;

    public RelatorioSondagemConsolidadoQuestaoUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoQuestaoPdf relatorioSondagemConsolidadoQuestaoPdf,
        IRelatorioSondagemConsolidadoGenericoExcel relatorioSondagemConsolidadoGenericoExcel,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoQuestaoUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular)
    {
        _pdf = relatorioSondagemConsolidadoQuestaoPdf;
        _excel = relatorioSondagemConsolidadoGenericoExcel;
    }

    protected override string AgrupamentoPadrao => "Por questão";

    protected override string ContextoRelatorioParaLog => "por questão";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorQuestaoAsync(filtros);

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _pdf.Executar(dadosRelatorio);

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _excel.GerarRelatorioExcelAsync(dadosRelatorio);

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
    }
}
