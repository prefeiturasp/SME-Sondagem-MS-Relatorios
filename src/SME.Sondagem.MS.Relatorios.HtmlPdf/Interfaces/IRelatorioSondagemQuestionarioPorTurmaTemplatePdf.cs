using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IRelatorioSondagemQuestionarioPorTurmaTemplatePdf
{
    string GerarHtml(RelatorioSondagemPorTurmaDto dto);
}
