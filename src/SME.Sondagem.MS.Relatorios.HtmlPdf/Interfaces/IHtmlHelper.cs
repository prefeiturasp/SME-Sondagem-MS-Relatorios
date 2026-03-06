namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IHtmlHelper
{
    Task<string> RenderRazorViewToString(string viewName, object model);
}
