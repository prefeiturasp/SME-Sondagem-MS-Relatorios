using SME.Sondagem.MS.Relatorios.Infra.Constantes;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf;

/// <summary>
/// Fragmentos CSS/HTML repetidos nos templates PDF de sondagem (wkhtml logo + meta-table).
/// </summary>
internal static class RelatorioSondagemPdfCabecalhoCompartilhado
{
    internal const string CssUniversalReset =
        "* { box-sizing: border-box; margin: 0; padding: 0; }";

    internal const string CssCabecalhoLogoEMetaRules =
        """
        .header-logo { margin-bottom: 6px; }
        .header-logo img { height: 36px; }
        .meta-table {
            width: 100%;
            border-collapse: collapse;
            border: 1px solid #D9D9D9;
        }
        .meta-table td {
            border: 1px solid #D9D9D9;
            padding: 4px 8px;
            font-size: 10px;
            font-weight: 400;
            line-height: 12px;
            vertical-align: middle;
            color: #42474A;
        }
        .meta-table td strong {
            font-weight: 700;
            margin-right: 2px;
        }
        """;

    private const string CssCabecalhoWkCorpoTituloRules =
        """
        body {
            font-family: 'Roboto', 'DejaVu Sans', Arial, sans-serif;
            font-size: 10px;
            color: #42474A;
            background: #fff;
            padding: 4px 16px 8px 16px;
            margin: 0;
            letter-spacing: 0.02em;
            word-spacing: 0.05em;
        }
        .report-title {
            text-align: center;
            margin-top: 8px;
            margin-bottom: 8px;
        }
        .report-title h1 {
            font-size: 14px;
            font-weight: 700;
            color: #42474A;
            margin-bottom: 2px;
            line-height: 1.2;
        }
        .report-title h2 {
            font-size: 10px;
            font-weight: 400;
            color: #42474A;
            line-height: 12px;
        }
        """;

    internal static readonly string MarkupTopoDocumentoCabecalhoWk =
        $"""
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset="utf-8" />
                <style>
                {CssUniversalReset}
                {CssCabecalhoWkCorpoTituloRules}
                {CssCabecalhoLogoEMetaRules}</style>
                </head>
                <body>
                """;

    internal static readonly string LogoPrefeituraHtml =
        $"""
            <div class="header-logo">
                <img src="{SmeConstants.Logo_PrefSP_Horizontal}" alt="Prefeitura de São Paulo" />
            </div>
            """;
}
