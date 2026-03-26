using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf;

public class ReportConverter : IReportConverter
{
    public readonly IConverter converter;

    public ReportConverter(IConverter converter)
    {
        this.converter = converter;
    }

    public byte[] GerarPdfEmMemoria(string html)
    {
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
            Objects = {
                new ObjectSettings() {
                        HtmlContent = html,
                        PagesCount = true,
                        FooterSettings = {
                        FontName="Roboto",
                        FontSize = 9,
                        Right = "[page] / [toPage]",
                        Left = $"SME - Sondagem",
                        },
                        WebSettings = {
                            DefaultEncoding = "utf-8",
                            Background = true
                                    }
                                }
                        }
        };

        return converter.Convert(doc);
    }
}
