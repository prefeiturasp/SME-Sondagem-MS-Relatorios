using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Records;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoSondagemApiClient
{
    Task<RetornoApiSondagemQuestionarioDto> ObterDadosQuestionarioAsync(FiltroRelatorioSondagemPorTurmaDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorRacaAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorGeneroAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorAnoAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorBimestreAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorQuestaoAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<RelatorioConsolidadoSondagemDto> ObterDadosRelatorioConsolidadoPorRacaGeneroAsync(FiltroRelatorioSondagemGenericoDto filtroRelatorio);
    Task<List<ParametroSondagemDto>?> ObterParametrosSondagemPorQuestionarioId(long questionoarioId, CancellationToken cancellationToken = default);
    Task<ProficienciaDto?> ObterProficienciaPorIdAsync(int proficienciaId, CancellationToken cancellationToken = default);
}
