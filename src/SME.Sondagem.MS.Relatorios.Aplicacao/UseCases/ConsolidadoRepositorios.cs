using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;

public record ConsolidadoRepositorios(
    IRepositorioComponenteCurricular ComponenteCurricular,
    IRepositorioGeneroSexo GeneroSexo,
    IRepositorioRacaCor RacaCor);
