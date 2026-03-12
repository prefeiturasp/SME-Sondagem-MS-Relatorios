using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaExcel
{
    Task<string> GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao);
}
