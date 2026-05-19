using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoGenericoExcel : IRelatorioSondagemConsolidadoGenericoExcel
{
    private readonly IRelatorioSondagemConsolidadoGenericoTemplateExcel _template;

    public RelatorioSondagemConsolidadoGenericoExcel(IRelatorioSondagemConsolidadoGenericoTemplateExcel template)
    {
        _template = template;
    }

    public async Task<string> GerarRelatorioExcelAsync(RelatorioConsolidadoSondagemDto relatorioConsolidadoSondagemDto)
    {
        return await _template.GerarExcelEF(relatorioConsolidadoSondagemDto);
    }
}
