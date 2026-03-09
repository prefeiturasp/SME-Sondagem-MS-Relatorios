using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaPdf
{
    Task<bool> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao);
}
