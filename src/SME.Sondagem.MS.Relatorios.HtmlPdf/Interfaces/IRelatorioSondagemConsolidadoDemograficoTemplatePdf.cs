using SME.Sondagem.MS.Relatorios.Infra.Dtos;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

/// <summary>
/// Templates consolidados que compartilham a base demográfica (HTML corpo + documento opcional para header WkHtml).
/// </summary>
public interface IRelatorioSondagemConsolidadoDemograficoTemplatePdf
{
    string GerarHtml(RelatorioConsolidadoSondagemDto dto);

    /// <summary>
    /// HTML completo só com filtros/logo a ser injetado em <see cref="IReportConverter.GerarPdfEmMemoria(string, string?)"/>
    /// via HeaderSettings (repetido no topo de cada página do PDF).
    /// </summary>
    string GerarHtmlDocumentoCabecalhoWk(RelatorioConsolidadoSondagemDto dto);
}
