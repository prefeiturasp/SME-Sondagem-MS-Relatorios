namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoSgpApiClient
{
    Task FinalizarSolicitacaoRelatorioAsync(int solicitacaoRelatorioId);
}
