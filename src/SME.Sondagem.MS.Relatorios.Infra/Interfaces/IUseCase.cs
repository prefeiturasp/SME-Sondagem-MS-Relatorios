namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IUseCase<in TParameter, TResponse>
{
    Task<TResponse> Executar(TParameter param);
}