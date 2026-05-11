using SME.Sondagem.MS.Relatorios.Dominio.Entidades;

namespace SME.Sondagem.MS.Relatorios.Dominio.Interfaces;

public interface IRepositorioComponenteCurricular
{
    Task<ComponenteCurricular> ObterPorIdAsync(int id);
}
