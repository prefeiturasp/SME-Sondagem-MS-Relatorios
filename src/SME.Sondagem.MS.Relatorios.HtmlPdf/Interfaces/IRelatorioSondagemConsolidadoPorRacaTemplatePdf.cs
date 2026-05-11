using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IRelatorioSondagemConsolidadoPorRacaTemplatePdf
{
    string GerarHtml(RelatorioConsolidadoSondagemDto dto);
}
