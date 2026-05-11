using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Excel.Interfaces;

public interface IRelatorioSondagemConsolidadoGenericoTemplateExcel
{
    Task<string> GerarExcelEF(RelatorioConsolidadoSondagemDto relatorioConsolidadoSondagemDto);
}
