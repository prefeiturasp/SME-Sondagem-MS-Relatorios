using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoEolApiClient
{
    Task<List<EscolaDto>> ObterDadosDreAsync(List<string> codigoUe);
    Task<TurmaDto> ObterDadosTurmaAsync(int codigoTurma);
    Task<DadosUsuarioDto> ObterDadosUsuarioAsync(string codigoRf);
}
