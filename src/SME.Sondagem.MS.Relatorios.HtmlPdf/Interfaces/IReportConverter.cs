namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IReportConverter
{
    byte[] GerarPdfEmMemoria(string html);
}
