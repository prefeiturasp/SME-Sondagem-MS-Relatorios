using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IRelatorioSondagemConsolidadoPorGeneroTemplatePdf
{
    string GerarHtml(RelatorioConsolidadoSondagemDto dto);
}
