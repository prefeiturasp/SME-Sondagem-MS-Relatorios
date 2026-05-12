using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemConsolidadoPorBimestreExcel : IRelatorioSondagemConsolidadoPorBimestreExcel
{
    private readonly IRelatorioSondagemConsolidadoPorBimestreTemplateExcel _relatorioSondagemConsolidadoPorBimestreTemplateExcel;

    public RelatorioSondagemConsolidadoPorBimestreExcel(IRelatorioSondagemConsolidadoPorBimestreTemplateExcel relatorioSondagemConsolidadoPorBimestreTemplateExcel)
    {
        _relatorioSondagemConsolidadoPorBimestreTemplateExcel = relatorioSondagemConsolidadoPorBimestreTemplateExcel;
    }

    public async Task<string> GerarRelatorioSondagemConsolidadoPorBimestreExcelAsync(RelatorioConsolidadoSondagemDto relatorioConsolidadoSondagemDto)
    {
        return await _relatorioSondagemConsolidadoPorBimestreTemplateExcel.GerarExcelEF(relatorioConsolidadoSondagemDto);
    }
}
