using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoGeneroUseCase
    : RelatorioSondagemConsolidadoUseCaseBase<RelatorioSondagemConsolidadoGeneroUseCase>,
      IRelatorioSondagemConsolidadoGeneroUseCase
{
    private readonly IRelatorioSondagemConsolidadoGeneroPdf _pdf;

    public RelatorioSondagemConsolidadoGeneroUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoGeneroPdf relatorioSondagemConsolidadoGeneroPdf,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoGeneroUseCase> logger)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger)
    {
        _pdf = relatorioSondagemConsolidadoGeneroPdf;
    }

    protected override string AgrupamentoPadrao => "Por gênero";

    protected override string ContextoRelatorioParaLog => "por gênero";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorGeneroAsync(filtros);

    protected override Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) =>
        _pdf.Executar(dadosRelatorio);
    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        throw new NotImplementedException();
    }

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Genero))
            dadosRelatorio.Genero = ValorTodos;
    }
}
