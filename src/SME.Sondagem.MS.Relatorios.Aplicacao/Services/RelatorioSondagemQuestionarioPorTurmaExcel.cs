using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaExcel : IRelatorioSondagemQuestionarioPorTurmaExcel
{
    private readonly IRelatorioSondagemQuestionarioPorTurmaTemplateExcel _relatorioSondagemQuestionarioPorTurmaTemplateExcel;

    public RelatorioSondagemQuestionarioPorTurmaExcel(IRelatorioSondagemQuestionarioPorTurmaTemplateExcel relatorioSondagemQuestionarioPorTurmaTemplateExcel)
    {
        _relatorioSondagemQuestionarioPorTurmaTemplateExcel = relatorioSondagemQuestionarioPorTurmaTemplateExcel;
    }

    public async Task<string> GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto)
    {
        return await _relatorioSondagemQuestionarioPorTurmaTemplateExcel.GerarExcelEF(relatorioSondagemPorTurmaDto);
    }
}