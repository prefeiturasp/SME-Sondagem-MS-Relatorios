using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemConsolidadoBimestrePdf
{
    Task<string> Executar(RelatorioConsolidadoSondagemDto dadosRelatorio);
}
