using SME.Sondagem.MS.Relatorios.Infra.Records;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaPdf
{
    Task<bool> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(RetornoApiSondagemQuestionarioDto retornoApiSondagemQuestionarioDto);
}
