using SME.Sondagem.MS.Relatorios.Dominio.Entidades;

namespace SME.Sondagem.MS.Relatorios.Dominio.Interfaces;

public interface IRepositorioGeneroSexo
{
    Task<IEnumerable<GeneroSexo>> ObterTodosAsync();
}
