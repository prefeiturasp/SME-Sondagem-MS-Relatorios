using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoRacaUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoRacaUseCase
{
    private readonly IRelatorioSondagemConsolidadoRacaPdf _pdf;

    public RelatorioSondagemConsolidadoRacaUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoRacaPdf relatorioSondagemConsolidadoRacaPdf,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoRacaUseCase> logger)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger)
    {
        _pdf = relatorioSondagemConsolidadoRacaPdf;
    }

    protected override string AgrupamentoPadrao => "Por raça";

    protected override string ContextoRelatorioParaLog => "por raça";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorRacaAsync(filtros);

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _pdf.Executar(dadosRelatorio);

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        throw new NotImplementedException();
    }

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Raca))
            dadosRelatorio.Raca = ValorTodas;
    }
}
