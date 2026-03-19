using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Excel.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaTemplateExcel
{
    Task<string> GerarExcelEF(RelatorioSondagemPorTurmaDto relatorioSondagemPorTurmaDto);
}
