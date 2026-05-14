namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IReportConverter
{
    /// <param name="html">Documento principal (corpo).</param>
    /// <param name="htmlDocumentoCabecalhoPorPaginaWk">
    /// Opcional: HTML completo renderizado pelo wkhtml como header em cada página (arquivo temporário).</param>
    byte[] GerarPdfEmMemoria(string html, string? htmlDocumentoCabecalhoPorPaginaWk = null);
}
