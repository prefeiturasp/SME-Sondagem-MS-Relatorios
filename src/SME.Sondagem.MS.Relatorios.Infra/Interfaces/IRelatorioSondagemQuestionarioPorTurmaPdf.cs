using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaPdf
{
    Task<string> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(RelatorioSondagemPorTurmaDto consultaSondagemPorTurmaDto);
}
