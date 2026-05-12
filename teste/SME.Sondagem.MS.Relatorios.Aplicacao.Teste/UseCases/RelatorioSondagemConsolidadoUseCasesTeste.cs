using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using BimestreEntidade = SME.Sondagem.MS.Relatorios.Dominio.Entidades.Bimestre;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using BimestreEnum = SME.Sondagem.MS.Relatorios.Dominio.Enums.Bimestre;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Text.Json;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.UseCases;

internal static class ConsolidadoRelatorioTesteHelper
{
    internal static readonly JsonSerializerOptions JsonOptions = JsonSerializerExtensions.ObterConfigSerializer();

    internal static MensagemRabbit CriarMensagemRabbit(MensagemSondagemGenericoDto dto, Guid? correlacao = null)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        return new MensagemRabbit(json, correlacao ?? Guid.NewGuid());
    }

    internal static MensagemSondagemGenericoDto MensagemPadrao(int extensaoRelatorio = (int)ExtensaoRelatorio.Pdf) => new()
    {
        FiltrosUsados = new FiltroRelatorioSondagemGenericoDto
        {
            ExtensaoRelatorio = extensaoRelatorio,
            ProficienciaId = 10,
            ComponenteCurricularId = 20,
            Modalidade = (int)Modalidade.Infantil,
            AnoLetivo = 2026,
            Ano = 3,
            UeCodigo = "UE001",
            BimestreId = 2
        },
        SolicitacaoRelatorioId = 99,
        UsuarioQueSolicitou = "RF123"
    };
}

public class RelatorioSondagemConsolidadoRacaUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _api = new();
    private readonly Mock<IRelatorioSondagemConsolidadoRacaPdf> _pdf = new();
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoExcel> _excel = new();
    private readonly Mock<IRepositorioRacaCor> _racas = new();
    private readonly Mock<IServicoSgpApiClient> _sgp = new();
    private readonly Mock<IServicoEolApiClient> _eol = new();
    private readonly Mock<IServicoMensageria> _mensageria = new();
    private readonly Mock<ILogger<RelatorioSondagemConsolidadoRacaUseCase>> _logger = new();
    private readonly Mock<IRepositorioComponenteCurricular> _componente = new();
    private readonly RelatorioSondagemConsolidadoRacaUseCase _sut;

    public RelatorioSondagemConsolidadoRacaUseCaseTeste()
    {
        _sut = new RelatorioSondagemConsolidadoRacaUseCase(
            _api.Object,
            _pdf.Object,
            _excel.Object,
            _racas.Object,
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
    public async Task Executar_DeveRetornarFalso_QuandoGeracaoPdfLancarExcecao()
    {
        ConfigurarFluxoSucessoApi(new RelatorioConsolidadoSondagemDto());
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ThrowsAsync(new InvalidOperationException("falha pdf"));

        var dto = ConsolidadoRelatorioTesteHelper.MensagemPadrao();
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(dto));

        resultado.Should().BeFalse();
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()), Times.Never);
    }

    [Fact]
    public async Task Executar_DeveRetornarVerdadeiro_QuandoExtensaoForXlsx()
    {
        ConfigurarFluxoSucessoApi(new RelatorioConsolidadoSondagemDto());
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Executar_DeveConcluirSemPdf_QuandoExtensaoNaoForPdfOuXlsx()
    {
        ConfigurarFluxoSucessoApi(new RelatorioConsolidadoSondagemDto());
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Html);

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg));

        resultado.Should().BeTrue();
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f => f.UrlRelatorio == string.Empty)), Times.Once);
    }

    [Fact]
    public async Task Executar_DevePopularMetadadosCabecalhoERacasDisponiveis_QuandoPdf()
    {
        var listaRacas = new List<RacaCor>
        {
            new() { Id = 1, Descricao = "Branca", CodigoEolRacaCor = 1 }
        };
        ConfigurarFluxoSucessoApi(new RelatorioConsolidadoSondagemDto());
        _racas.Setup(x => x.ObterTodosAsync()).ReturnsAsync(listaRacas);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("https://pdf");

        var correlacao = Guid.NewGuid();
        var msg = ConsolidadoRelatorioTesteHelper.MensagemPadrao();
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(msg, correlacao));

        resultado.Should().BeTrue();
        capturado.Should().NotBeNull();
        capturado!.CodigoCorrelacao.Should().Be(correlacao);
        capturado.Agrupamento.Should().Be("Por raça");
        capturado.Raca.Should().Be("Todas");
        capturado.AnoLetivo.Should().Be(2026);
        capturado.Modalidade.Should().Be(Modalidade.Infantil.ShortName());
        capturado.UnidadeEducacional.Should().Be("UE001");
        capturado.AnoTurma.Should().Be("3° ANO");
        capturado.Bimestre.Should().Be(BimestreEnum.Primeiro.ShortName());
        capturado.Dre.Should().Be("Todas");
        capturado.Proficiencia.Should().Be("Proficiência X");
        capturado.ComponenteCurricular.Should().Be("Língua Portuguesa");
        capturado.Titulo.Should().Be("Proficiência X consolidado");
        capturado.Usuario.Should().Be("Fulano (RF123)");
        capturado.RacasDisponiveis.Should().BeEquivalentTo(listaRacas);

        _api.Verify(x => x.ObterDadosRelatorioConsolidadoPorRacaAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()), Times.Once);
        _racas.Verify(x => x.ObterTodosAsync(), Times.Once);
        _mensageria.Verify(x => x.Publicar(
            It.Is<MensagemRabbit>(m =>
                m.Mensagem != null &&
                m.Mensagem.GetType() == typeof(MensagemRelatorioProntoDto) &&
                ((MensagemRelatorioProntoDto)m.Mensagem).MensagemUsuario.Contains("Por raça")),
            RotasRabbit.RotaRelatoriosProntosSgp,
            ExchangeRabbit.Sgp,
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Executar_DevePreservarAgrupamentoERaca_QuandoApiJaPreencheu()
    {
        var dtoApi = new RelatorioConsolidadoSondagemDto
        {
            Agrupamento = "Grupo manual",
            Raca = "Parda"
        };
        ConfigurarFluxoSucessoApi(dtoApi);
        _racas.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.Agrupamento.Should().Be("Grupo manual");
        capturado.Raca.Should().Be("Parda");
    }

    [Fact]
    public async Task Executar_DeveManterUsuarioVazio_QuandoEolNaoRetornarUsuario()
    {
        ConfigurarFluxoSucessoApi(new RelatorioConsolidadoSondagemDto());
        _racas.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.Usuario.Should().BeEmpty();
    }

    private void ConfigurarFluxoSucessoApi(RelatorioConsolidadoSondagemDto retorno)
    {
        _api.Setup(x => x.ObterDadosRelatorioConsolidadoPorRacaAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()))
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
    }
}

public class RelatorioSondagemConsolidadoGeneroUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _api = new();
    private readonly Mock<IRelatorioSondagemConsolidadoGeneroPdf> _pdf = new();
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoExcel> _excel = new();
    private readonly Mock<IRepositorioGeneroSexo> _generos = new();
    private readonly Mock<IServicoSgpApiClient> _sgp = new();
    private readonly Mock<IServicoEolApiClient> _eol = new();
    private readonly Mock<IServicoMensageria> _mensageria = new();
    private readonly Mock<ILogger<RelatorioSondagemConsolidadoGeneroUseCase>> _logger = new();
    private readonly Mock<IRepositorioComponenteCurricular> _componente = new();
    private readonly RelatorioSondagemConsolidadoGeneroUseCase _sut;

    public RelatorioSondagemConsolidadoGeneroUseCaseTeste()
    {
        _sut = new RelatorioSondagemConsolidadoGeneroUseCase(
            _api.Object,
            _pdf.Object,
            _excel.Object,
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
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx)));
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Executar_DeveCarregarGenerosDisponiveis_QuandoPdf()
    {
        var lista = new List<GeneroSexo>
        {
            new() { Id = 1, Descricao = "Feminino", Sigla = "F" }
        };
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _generos.Setup(x => x.ObterTodosAsync()).ReturnsAsync(lista);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.GenerosDisponiveis.Should().BeEquivalentTo(lista);
        capturado.Agrupamento.Should().Be("Por gênero");
        capturado.Genero.Should().Be("Todos");
        _api.Verify(x => x.ObterDadosRelatorioConsolidadoPorGeneroAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()), Times.Once);
        _generos.Verify(x => x.ObterTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task Executar_DevePreservarGenero_QuandoApiJaPreencheu()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto { Genero = "Feminino" });
        _generos.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.Genero.Should().Be("Feminino");
    }

    [Fact]
    public async Task Executar_DeveConcluirSemPdf_QuandoExtensaoNaoForPdfOuXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Html)));

        resultado.Should().BeTrue();
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f => f.UrlRelatorio == string.Empty)), Times.Once);
    }

    private void ConfigurarFluxo(RelatorioConsolidadoSondagemDto retorno)
    {
        _api.Setup(x => x.ObterDadosRelatorioConsolidadoPorGeneroAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()))
            .ReturnsAsync(retorno);
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProficienciaDto { Nome = "P" });
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(new DadosUsuarioDto { Nome = "U" });
        _componente.Setup(x => x.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComponenteCurricular { Nome = "C" });
        _sgp.Setup(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()))
            .Returns(Task.CompletedTask);
        _mensageria.Setup(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>())).ReturnsAsync("url");
        _generos.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
    }
}

public class RelatorioSondagemConsolidadoQuestaoUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _api = new();
    private readonly Mock<IRelatorioSondagemConsolidadoQuestaoPdf> _pdf = new();
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoExcel> _excel = new();
    private readonly Mock<IServicoSgpApiClient> _sgp = new();
    private readonly Mock<IServicoEolApiClient> _eol = new();
    private readonly Mock<IServicoMensageria> _mensageria = new();
    private readonly Mock<ILogger<RelatorioSondagemConsolidadoQuestaoUseCase>> _logger = new();
    private readonly Mock<IRepositorioComponenteCurricular> _componente = new();
    private readonly RelatorioSondagemConsolidadoQuestaoUseCase _sut;

    public RelatorioSondagemConsolidadoQuestaoUseCaseTeste()
    {
        _sut = new RelatorioSondagemConsolidadoQuestaoUseCase(
            _api.Object,
            _pdf.Object,
            _excel.Object,
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
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx)));
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Executar_DeveDelegarPdfSemRepositorioDemograficoAdicional_QuandoPdf()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.Agrupamento.Should().Be("Por questão");
        _api.Verify(x => x.ObterDadosRelatorioConsolidadoPorQuestaoAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()), Times.Once);
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveConcluirSemPdf_QuandoExtensaoNaoForPdfOuXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Html)));

        resultado.Should().BeTrue();
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f => f.UrlRelatorio == string.Empty)), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveMontarTituloComProficienciaVazia_QuandoProficienciaNaoExistir()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProficienciaDto?)null);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.Titulo.Should().Be(" consolidado");
        capturado.Proficiencia.Should().BeEmpty();
    }

    private void ConfigurarFluxo(RelatorioConsolidadoSondagemDto retorno)
    {
        _api.Setup(x => x.ObterDadosRelatorioConsolidadoPorQuestaoAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()))
            .ReturnsAsync(retorno);
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProficienciaDto { Nome = "P" });
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(new DadosUsuarioDto { Nome = "U" });
        _componente.Setup(x => x.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComponenteCurricular { Nome = "C" });
        _sgp.Setup(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()))
            .Returns(Task.CompletedTask);
        _mensageria.Setup(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>())).ReturnsAsync("url");
    }
}

public class RelatorioSondagemConsolidadoBimestreUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _api = new();
    private readonly Mock<IRelatorioSondagemConsolidadoBimestrePdf> _pdf = new();
    private readonly Mock<IRepositorioBimestre> _bimestres = new();
    private readonly Mock<IServicoSgpApiClient> _sgp = new();
    private readonly Mock<IServicoEolApiClient> _eol = new();
    private readonly Mock<IServicoMensageria> _mensageria = new();
    private readonly Mock<ILogger<RelatorioSondagemConsolidadoBimestreUseCase>> _logger = new();
    private readonly Mock<IRepositorioComponenteCurricular> _componente = new();
    private readonly RelatorioSondagemConsolidadoBimestreUseCase _sut;

    public RelatorioSondagemConsolidadoBimestreUseCaseTeste()
    {
        _sut = new RelatorioSondagemConsolidadoBimestreUseCase(
            _api.Object,
            _pdf.Object,
            _bimestres.Object,
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
    public async Task Executar_DeveRetornarFalso_QuandoExtensaoForXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Xlsx)));
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Executar_DeveCarregarBimestresDisponiveis_QuandoPdf()
    {
        var lista = new List<BimestreEntidade> { new BimestreEntidade() };
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _bimestres.Setup(x => x.ObterTodosAsync()).ReturnsAsync(lista);

        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(ConsolidadoRelatorioTesteHelper.MensagemPadrao()));

        capturado.Should().NotBeNull();
        capturado!.BimestresDisponiveis.Should().BeEquivalentTo(lista);
        capturado.Agrupamento.Should().Be("Por bimestre");
        _api.Verify(x => x.ObterDadosRelatorioConsolidadoPorBimestreAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()), Times.Once);
        _bimestres.Verify(x => x.ObterTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveUsarValorTodosParaUeAnoTurmaEBimestre_QuandoApiRetornarVazioEUeSemCodigo()
    {
        var filtros = ConsolidadoRelatorioTesteHelper.MensagemPadrao();
        filtros.FiltrosUsados.UeCodigo = string.Empty;
        filtros.FiltrosUsados.Ano = 0;
        filtros.FiltrosUsados.BimestreId = null;

        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _bimestres.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(filtros));

        capturado.Should().NotBeNull();
        capturado!.UnidadeEducacional.Should().Be("Todas");
        capturado.AnoTurma.Should().Be("Todos");
        capturado.Bimestre.Should().Be("Todos");
    }

    [Fact]
    public async Task Executar_DeveConcluirSemPdf_QuandoExtensaoNaoForPdfOuXlsx()
    {
        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());

        var resultado = await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(
            ConsolidadoRelatorioTesteHelper.MensagemPadrao((int)ExtensaoRelatorio.Html)));

        resultado.Should().BeTrue();
        _pdf.Verify(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Never);
        _bimestres.Verify(x => x.ObterTodosAsync(), Times.Never);
        _sgp.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(
            It.Is<FinalizarSolicitacaoRelatorioDto>(f => f.UrlRelatorio == string.Empty)), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveUsarTodosParaBimestre_QuandoBimestreIdNaoCorresponderAoEnum()
    {
        var filtros = ConsolidadoRelatorioTesteHelper.MensagemPadrao();
        filtros.FiltrosUsados.BimestreId = 999;

        ConfigurarFluxo(new RelatorioConsolidadoSondagemDto());
        _bimestres.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
        RelatorioConsolidadoSondagemDto? capturado = null;
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _sut.Executar(ConsolidadoRelatorioTesteHelper.CriarMensagemRabbit(filtros));

        capturado.Should().NotBeNull();
        capturado!.Bimestre.Should().Be("Todos");
    }

    private void ConfigurarFluxo(RelatorioConsolidadoSondagemDto retorno)
    {
        _api.Setup(x => x.ObterDadosRelatorioConsolidadoPorBimestreAsync(It.IsAny<FiltroRelatorioSondagemGenericoDto>()))
            .ReturnsAsync(retorno);
        _api.Setup(x => x.ObterProficienciaPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProficienciaDto { Nome = "P" });
        _eol.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(new DadosUsuarioDto { Nome = "U" });
        _componente.Setup(x => x.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ComponenteCurricular { Nome = "C" });
        _sgp.Setup(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()))
            .Returns(Task.CompletedTask);
        _mensageria.Setup(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _pdf.Setup(x => x.Executar(It.IsAny<RelatorioConsolidadoSondagemDto>())).ReturnsAsync("url");
        _bimestres.Setup(x => x.ObterTodosAsync()).ReturnsAsync([]);
    }
}
