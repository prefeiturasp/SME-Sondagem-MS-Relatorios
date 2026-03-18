using FluentAssertions;
using Moq;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Teste;

public class ReportConverterTeste
{
    private readonly Mock<IConverter> _converterMock;
    private readonly ReportConverter _reportConverter;

    public ReportConverterTeste()
    {
        _converterMock = new Mock<IConverter>();
        _reportConverter = new ReportConverter(_converterMock.Object);
    }

    [Fact]
    public void GerarPdfEmMemoria_DeveConverterHtmlEmPdfCorretamente()
    {
        // Arrange
        var html = "<html><body><h1>Teste</h1></body></html>";
        var bytesEsperados = new byte[] { 1, 2, 3, 4 };

        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = "<h1>Relatório de Sondagem</h1><p>Conteúdo do gráfico...</p>",
            WebSettings = { DefaultEncoding = "utf-8" },
            HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Página [page] de [toPage]", Line = true },
            FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Relatório Gerado por Sistema" }
        };

        IDocument documentoEnviado = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        _converterMock.Setup(c => c.Convert(It.IsAny<IDocument>()))
            .Callback<IDocument>(doc => documentoEnviado = doc)
            .Returns(bytesEsperados);

        // Act
        var result = _reportConverter.GerarPdfEmMemoria(html);

        // Assert
        result.Should().BeEquivalentTo(bytesEsperados);
        
        documentoEnviado.Should().NotBeNull();
        var htmlDoc = documentoEnviado as HtmlToPdfDocument;
        htmlDoc.Should().NotBeNull();
        htmlDoc.GlobalSettings.ColorMode.Should().Be(ColorMode.Color);
        htmlDoc.GlobalSettings.Orientation.Should().Be(Orientation.Portrait);

        var objSettings = htmlDoc.Objects.FirstOrDefault();
        objSettings.Should().NotBeNull();
        objSettings.HtmlContent.Should().Be(html);
        objSettings.PagesCount.Should().BeTrue();
        
        // Footer settings
        objSettings.FooterSettings.FontName.Should().Be("Roboto");
        objSettings.FooterSettings.FontSize.Should().Be(9);
        objSettings.FooterSettings.Right.Should().Be("[page] / [toPage]");
        objSettings.FooterSettings.Left.Should().Be("SME - Sondagem");
        
        // Web settings
        objSettings.WebSettings.DefaultEncoding.Should().Be("utf-8");
        objSettings.WebSettings.Background.Should().BeTrue();
        
        _converterMock.Verify(c => c.Convert(It.IsAny<HtmlToPdfDocument>()), Times.Once);
    }

    [Fact]
    public void GerarPdfEmMemoria_DeveRepassarExcecaoDoConverter()
    {
        // Arrange
        var html = "<html></html>";
        _converterMock.Setup(c => c.Convert(It.IsAny<IDocument>()))
            .Throws(new System.Exception("Erro ao converter"));

        // Act
        var act = () => _reportConverter.GerarPdfEmMemoria(html);

        // Assert
        act.Should().Throw<System.Exception>().WithMessage("Erro ao converter");
    }
}
