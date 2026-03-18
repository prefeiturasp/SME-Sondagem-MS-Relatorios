using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class SemestreOuBimestreTeste
{
    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarSemestreTodos_QuandoEjaESemestreVazio()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(string.Empty, string.Empty, Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarSemestreZero_QuandoEjaESemestreZero()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(string.Empty, "0", Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarPrimeiro_QuandoEjaESemestre1()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(string.Empty, "1", Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Primeiro");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarSegundo_QuandoEjaESemestre2()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(string.Empty, "2", Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Segundo");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreTodos_QuandoFundamentalEBimestreVazio()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(string.Empty, string.Empty, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreZero_QuandoFundamentalEBimestreZero()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre("0", string.Empty, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }

    [Theory]
    [InlineData("2", "Primeiro")]
    [InlineData("3", "Segundo")]
    [InlineData("4", "Terceiro")]
    [InlineData("5", "Quarto")]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreCorreto_QuandoFundamental(string bimestreValor, string nomeBimestreEsperado)
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(bimestreValor, string.Empty, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be(nomeBimestreEsperado);
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreTodos_QuandoMedioEBimestreInvalido()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre("invalido", string.Empty, Modalidade.Medio);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }
}
