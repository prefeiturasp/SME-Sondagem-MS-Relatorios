using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemConsolidadoGenericoExcel
{
    Task<string> GerarRelatorioExcelAsync(RelatorioConsolidadoSondagemDto relatorioConsolidadoSondagemDto);
}
