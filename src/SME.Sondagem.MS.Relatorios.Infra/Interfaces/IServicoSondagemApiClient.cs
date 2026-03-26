using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Records;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoSondagemApiClient
{
    Task<RetornoApiSondagemQuestionarioDto> ObterDadosQuestionarioAsync(FiltroRelatorioSondagemPorTurmaDto filtroRelatorio);
    Task<List<ParametroSondagemDto>?> ObterParametrosSondagemPorQuestionarioId(long questionoarioId, CancellationToken cancellationToken = default);
    Task<ProficienciaDto?> ObterProficienciaPorIdAsync(int proficienciaId, CancellationToken cancellationToken = default);
}
