using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaExcel
{
    Task<bool> GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto);
}
