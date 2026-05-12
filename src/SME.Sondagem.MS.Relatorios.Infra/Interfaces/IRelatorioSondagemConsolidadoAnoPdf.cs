using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemConsolidadoAnoPdf
{
    Task<string> Executar(RelatorioConsolidadoSondagemDto consultaSondagemPorTurmaDto);
}
