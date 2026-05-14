using System.Text;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf;

public class ReportConverter : IReportConverter
{
    /// <summary>
    /// Margem superior (mm) reservada ao header HTML do wkhtml; ajuste se filtros opcionais fizerem o cabeçalho ficar maior.
    /// </summary>
    private const double MargemSuperiorMmComCabecalhoHtml = 54;

    public readonly IConverter converter;

    public ReportConverter(IConverter converter)
    {
        this.converter = converter;
    }

    public byte[] GerarPdfEmMemoria(string html, string? htmlDocumentoCabecalhoPorPaginaWk = null)
    {
        string? caminhoCabecalhoTemp = null;
        var objectSettings = new ObjectSettings
        {
            HtmlContent = html,
            PagesCount = true,
            FooterSettings =
            {
                FontName = "Roboto",
                FontSize = 9,
                Right = "[page] / [toPage]",
                Left = $"SME - Sondagem",
            },
            WebSettings =
            {
                DefaultEncoding = "utf-8",
                Background = true,
            },
        };

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
            Objects = { objectSettings },
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(htmlDocumentoCabecalhoPorPaginaWk))
            {
                caminhoCabecalhoTemp = Path.Combine(Path.GetTempPath(), $"sme-relatorio-pdf-header-{Guid.NewGuid():N}.html");
                File.WriteAllText(caminhoCabecalhoTemp, htmlDocumentoCabecalhoPorPaginaWk, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                doc.GlobalSettings.Margins ??= new MarginSettings();
                doc.GlobalSettings.Margins.Top = MargemSuperiorMmComCabecalhoHtml;
                objectSettings.LoadSettings ??= new LoadSettings();
                objectSettings.LoadSettings.BlockLocalFileAccess = false;
                objectSettings.HeaderSettings = new HeaderSettings
                {
                    HtmlUrl = new Uri(caminhoCabecalhoTemp).AbsoluteUri,
                    Spacing = 3,
                    FontName = "Roboto",
                    FontSize = 9,
                };
            }

            return converter.Convert(doc);
        }
        finally
        {
            if (caminhoCabecalhoTemp != null)
            {
                try
                {
                    if (File.Exists(caminhoCabecalhoTemp))
                        File.Delete(caminhoCabecalhoTemp);
                }
                catch (IOException)
                {
                    // arquivo ainda pode estar em uso pelo wkhtml neste ciclo — ignorado
                }
            }
        }
    }
}
