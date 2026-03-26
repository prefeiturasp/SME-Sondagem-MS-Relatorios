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
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(null, null, Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarSemestreZero_QuandoEjaESemestreZero()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(null, 0, Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarPrimeiro_QuandoEjaESemestre1()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(null, 1, Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("1° Semestre");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarSegundo_QuandoEjaESemestre2()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(null, 2, Modalidade.EJA);

        nomeFiltro.Should().Be("Semestre");
        valorFiltro.Should().Be("2° Semestre");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreTodos_QuandoFundamentalEBimestreVazio()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(null, null, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreZero_QuandoFundamentalEBimestreZero()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(0, null, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }

    [Theory]
    [InlineData(2, "1° Bimestre")]
    [InlineData(3, "2° Bimestre")]
    [InlineData(4, "3° Bimestre")]
    [InlineData(5, "4° Bimestre")]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreCorreto_QuandoFundamental(int bimestreValor, string nomeBimestreEsperado)
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(bimestreValor, null, Modalidade.Fundamental);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be(nomeBimestreEsperado);
    }

    [Fact]
    public void ObterFiltroSemestreOuBimestre_DeveRetornarBimestreTodos_QuandoMedioEBimestreInvalido()
    {
        var (nomeFiltro, valorFiltro) = SemestreOuBimestre.ObterFiltroSemestreOuBimestre(-1, null, Modalidade.Medio);

        nomeFiltro.Should().Be("Bimestre");
        valorFiltro.Should().Be("Todos");
    }
}
