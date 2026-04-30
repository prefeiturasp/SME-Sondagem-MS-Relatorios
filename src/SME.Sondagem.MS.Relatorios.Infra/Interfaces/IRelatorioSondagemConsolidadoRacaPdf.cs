using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemConsolidadoRacaPdf
{
    Task<string> Executar(RelatorioConsolidadoSondagemDto consultaSondagemPorTurmaDto);
}
