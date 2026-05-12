using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.UseCases;

public class RelatorioSondagemConsolidadoRacaGeneroUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _api = new();
    private readonly Mock<IRelatorioSondagemConsolidadoRacaGeneroPdf> _pdf = new();
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoExcel> _excel = new();
    private readonly Mock<IRepositorioRacaCor> _racas = new();
    private readonly Mock<IRepositorioGeneroSexo> _generos = new();
    private readonly Mock<IServicoSgpApiClient> _sgp = new();
    private readonly Mock<IServicoEolApiClient> _eol = new();
    private readonly Mock<IServicoMensageria> _mensageria = new();
    private readonly Mock<ILogger<RelatorioSondagemConsolidadoRacaGeneroUseCase>> _logger = new();
    private readonly Mock<IRepositorioComponenteCurricular> _componente = new();
    private readonly RelatorioSondagemConsolidadoRacaGeneroUseCase _sut;

    public RelatorioSondagemConsolidadoRacaGeneroUseCaseTeste()
    {
        _sut = new RelatorioSondagemConsolidadoRacaGeneroUseCase(
            _api.Object,
            _pdf.Object,
            _excel.Object,
            _racas.Object,
            _generos.Object,
            _sgp.Object,
            _eol.Object,
            _mensageria.Object,
            _logger.Object,
            _componente.Object);
    }

    [Fact]
    public async Task Executar_DeveRetornarFalso_QuandoCorpoDaMensagemForInvalido()
    {
        var resultado = await _sut.Executar(new MensagemRabbit(null, Guid.NewGuid()));
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Executar_DeveRetornarVerdadeiro_QuandoExtensaoForXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeTrue();
        _excel.Verify(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveRetornarVerdadeiro_QuandoExtensaoForPdf()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Pdf);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeTrue();
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Once);
        _excel.Verify(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
    }

    [Fact]
    public async Task Executar_DeveConcluirSemArquivo_QuandoExtensaoNaoForPdfOuXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Html);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeTrue();
        _excel.Verify(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f => f.UrlRelatorio == string.Empty)), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveRetornarFalso_QuandoExcelLancarExcecao()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ThrowsAsync(new InvalidOperationException("falha excel"));
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeFalse();
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()), Times.Never);
    }

    [Fact]
    public async Task Executar_DevePopularMetadadosCabecalho_QuandoExcel()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());

        RelatorioConsolidadoSondagemDto? capturado = null;
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("https://excel");

        var correlacao = Guid.NewGuid();
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg, correlacao));

        resultado.Should().BeTrue();
        capturado.Should().NotBeNull();
        capturado!.CodigoCorrelacao.Should().Be(correlacao);
        capturado.Agrupamento.Should().Be("Por raça e gênero");
        capturado.AnoLetivo.Should().Be(2026);
        capturado.Modalidade.Should().Be(Modalidade.Infantil.ShortName());
        capturado.UnidadeEducacional.Should().Be("UE001");
        capturado.AnoTurma.Should().Be("3° ANO");
        capturado.Dre.Should().Be("Todas");
        capturado.Proficiencia.Should().Be("Proficiência X");
        capturado.ComponenteCurricular.Should().Be("Língua Portuguesa");
        capturado.Titulo.Should().Be("Proficiência X consolidado");
        capturado.Usuario.Should().Be("Fulano (RF123)");
        capturado.Genero.Should().Be("Todos");
        capturado.Raca.Should().Be("Todas");

        _api.Verify(x => x.ObterDadosRelatorioConsolidadoPorRacaGeneroAsync(
            It.IsAny<FiltroRelatorioSondagemGenericoDto>()), Times.Once);
        _mensageria.Verify(x => x.Publicar(
            It.Is<MensagemRabbit>(m =>
                m.Mensagem != null &&
                m.Mensagem.GetType() == typeof(MensagemRelatorioProntoDto) &&
                ((MensagemRelatorioProntoDto)m.Mensagem).MensagemUsuario.Contains("Por raça e gênero")),
            RotasRabbit.RotaRelatoriosProntosSgp,
            ExchangeRabbit.Sgp,
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Executar_DevePreservarAgrupamento_QuandoApiJaPreencheu()
    {
        var dtoApi = new RelatorioConsolidadoSondagemDto { Agrupamento = "Agrupamento customizado" };
        ConfigurarFluxo(dtoApi);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        capturado.Should().NotBeNull();
        capturado!.Agrupamento.Should().Be("Agrupamento customizado");
    }

    [Fact]
    public async Task Executar_DeveManterUsuarioVazio_QuandoEolNaoRetornarUsuario()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        capturado.Should().NotBeNull();
        capturado!.Usuario.Should().BeEmpty();
    }

    [Fact]
    public async Task Executar_DeveMontarTituloComProficienciaVazia_QuandoProficienciaNaoExistir()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProficienciaDto?)null);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        capturado.Should().NotBeNull();
        capturado!.Titulo.Should().Be(" consolidado");
        capturado.Proficiencia.Should().BeEmpty();
    }

    [Fact]
    public async Task Executar_DeveUsarValorTodosParaUeAnoTurmaEBimestre_QuandoFiltrosVazios()
    {
        var filtros = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        filtros.FiltrosUsados.UeCodigo = string.Empty;
        filtros.FiltrosUsados.Ano = 0;
        filtros.FiltrosUsados.BimestreId = null;

        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());

        RelatorioConsolidadoSondagemDto? capturado = null;
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(filtros));

        capturado.Should().NotBeNull();
        capturado!.UnidadeEducacional.Should().Be("Todas");
        capturado.AnoTurma.Should().Be("Todos");
        capturado.Bimestre.Should().Be("Todos");
    }

    [Fact]
    public async Task Executar_DeveFinalizarRelatorioNoSgp_QuandoExcelGeradoComSucesso()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        var linkGerado = "https://minio/relatorio.xlsx";
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ReturnsAsync(linkGerado);

        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);
        msg.SolicitacaoRelatorioId = 42;

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f =>
                f.SolicitacaoRelatorioId == 42 &&
                f.UrlRelatorio == linkGerado)), Times.Once);
    }

    private void ConfigurarFluxo(RelatorioConsolidadoSondagemDto retorno)
    {
        _api.Setup(x => x.ObterDadosRelatorioConsolidadoPorRacaGeneroAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()))
            .ReturnsAsync(retorno);
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProficienciaDto { Nome = "Proficiência X" });
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(new DadosUsuarioDto { Nome = "Fulano" });
        _componente.Setup(x => x.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComponenteCurricular { Nome = "Língua Portuguesa" });
        _sgp.Setup(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()))
            .Returns(Task.CompletedTask);
        _mensageria.Setup(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>())).ReturnsAsync("https://pdf");
        _excel.Setup(x => x.GerarRelatorioExcelAsync(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ReturnsAsync("https://excel");
        _racas.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        _generos.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
    }
}
