using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoSgpApiClient
{
    Task FinalizarSolicitacaoRelatorioAsync(FinalizarSolicitacaoRelatorioDto finalizarSolicitacaoRelatorioDto);
}
