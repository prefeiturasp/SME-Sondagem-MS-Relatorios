using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class EnumExtensaoTeste
{
    [Theory]
    [InlineData(Modalidade.Infantil, "EI")]
    [InlineData(Modalidade.EJA, "EJA")]
    [InlineData(Modalidade.Fundamental, "EF")]
    [InlineData(Modalidade.Medio, "EM")]
    [InlineData(Modalidade.CELP, "CELP")]
    public void ShortName_DeveRetornarShortNameCorreto_QuandoModalidadeValida(Modalidade modalidade, string shortNameEsperado)
    {
        var resultado = modalidade.ShortName();

        resultado.Should().Be(shortNameEsperado);
    }

    [Theory]
    [InlineData(Bimestre.Inicial, "Inicial")]
    [InlineData(Bimestre.Primeiro, "1° bimestre")]
    [InlineData(Bimestre.Segundo, "2° bimestre")]
    [InlineData(Bimestre.Terceiro, "3° bimestre")]
    [InlineData(Bimestre.Quarto, "4° bimestre")]
    public void ShortName_DeveRetornarShortNameCorreto_QuandoBimestreValido(Bimestre bimestre, string shortNameEsperado)
    {
        var resultado = bimestre.ShortName();

        resultado.Should().Be(shortNameEsperado);
    }

    [Theory]
    [InlineData(Semestre.Primeiro, "1° semestre")]
    [InlineData(Semestre.Segundo, "2° semestre")]
    public void ShortName_DeveRetornarShortNameCorreto_QuandoSemestreValido(Semestre semestre, string shortNameEsperado)
    {
        var resultado = semestre.ShortName();

        resultado.Should().Be(shortNameEsperado);
    }

    [Fact]
    public void GetEnumByShortName_DeveRetornarModalidade_QuandoShortNameValido()
    {
        var resultado = EnumExtensao.GetEnumByShortName<Modalidade>("EF");

        resultado.Should().Be(Modalidade.Fundamental);
    }

    [Fact]
    public void GetEnumByShortName_DeveRetornarNulo_QuandoShortNameInexistente()
    {
        var resultado = EnumExtensao.GetEnumByShortName<Modalidade>("INEXISTENTE");

        resultado.Should().BeNull();
    }

    [Theory]
    [InlineData("EI", Modalidade.Infantil)]
    [InlineData("EJA", Modalidade.EJA)]
    [InlineData("EF", Modalidade.Fundamental)]
    [InlineData("EM", Modalidade.Medio)]
    [InlineData("CELP", Modalidade.CELP)]
    public void TryObterModalidadePorShortName_DeveRetornarVerdadeiro_EPreencherModalidade_QuandoShortNameValido(string shortName, Modalidade modalidadeEsperada)
    {
        var encontrou = EnumExtensao.TryObterModalidadePorShortName(shortName, out var modalidade);

        encontrou.Should().BeTrue();
        modalidade.Should().Be(modalidadeEsperada);
    }

    [Fact]
    public void TryObterModalidadePorShortName_DeveRetornarFalso_QuandoShortNameInvalido()
    {
        var encontrou = EnumExtensao.TryObterModalidadePorShortName("INVALIDO", out var modalidade);

        encontrou.Should().BeFalse();
        modalidade.Should().Be(default(Modalidade));
    }
}
