using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoQuestaoUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoQuestaoUseCase
{
    private readonly IRelatorioSondagemConsolidadoQuestaoPdf _pdf;

    public RelatorioSondagemConsolidadoQuestaoUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoQuestaoPdf relatorioSondagemConsolidadoQuestaoPdf,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoQuestaoUseCase> logger)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger)
    {
        _pdf = relatorioSondagemConsolidadoQuestaoPdf;
    }

    protected override string AgrupamentoPadrao => "Por questão";

    protected override string ContextoRelatorioParaLog => "por questão";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorQuestaoAsync(filtros);

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _pdf.Executar(dadosRelatorio);

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) => throw new NotImplementedException();

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
    }
}
