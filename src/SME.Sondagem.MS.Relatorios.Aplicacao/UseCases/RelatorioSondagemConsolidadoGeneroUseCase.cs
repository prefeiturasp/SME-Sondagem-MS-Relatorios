using Microsoft.Extensions.Logging;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public class RelatorioSondagemConsolidadoGeneroUseCase : RelatorioSondagemConsolidadoUseCaseBase, IRelatorioSondagemConsolidadoGeneroUseCase
{
    private readonly IRelatorioSondagemConsolidadoGeneroPdf _pdf;
    private readonly IRepositorioGeneroSexo _repositorioGeneroSexo;

    public RelatorioSondagemConsolidadoGeneroUseCase(
        IServicoSondagemApiClient servicoSondagemApiClient,
        IRelatorioSondagemConsolidadoGeneroPdf relatorioSondagemConsolidadoGeneroPdf,
        IRepositorioGeneroSexo repositorioGeneroSexo,
        IServicoSgpApiClient servicoSgpApiClient,
        IServicoEolApiClient servicoEolApiClient,
        IServicoMensageria servicoMensageria,
        ILogger<RelatorioSondagemConsolidadoGeneroUseCase> logger,
        IRepositorioComponenteCurricular repositorioComponenteCurricular)
        : base(servicoSondagemApiClient, servicoSgpApiClient, servicoEolApiClient, servicoMensageria, logger, repositorioComponenteCurricular)
    {
        _pdf = relatorioSondagemConsolidadoGeneroPdf;
        _repositorioGeneroSexo = repositorioGeneroSexo;
    }

    protected override string AgrupamentoPadrao => "Por gênero";

    protected override string ContextoRelatorioParaLog => "por gênero";

    protected override Task<RelatorioConsolidadoSondagemDto> ObterRelatorioConsolidadoAsync(FiltroRelatorioSondagemPorTurmaDto filtros) =>
        ServicoSondagemApiClient.ObterDadosRelatorioConsolidadoPorGeneroAsync(filtros);

    protected override async Task<string> GerarPdfAsync(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        dadosRelatorio.GenerosDisponiveis = await _repositorioGeneroSexo.ObterTodosAsync();
        return await _pdf.Executar(dadosRelatorio);
    }

    protected override Task<string> GerarExcelAsync(RelatorioConsolidadoSondagemDto dadosRelatorio) => throw new NotImplementedException();

    protected override void GarantirMetadadoDemograficoPadrao(RelatorioConsolidadoSondagemDto dadosRelatorio)
    {
        if (string.IsNullOrWhiteSpace(dadosRelatorio.Genero))
            dadosRelatorio.Genero = ValorTodos;
    }
}
