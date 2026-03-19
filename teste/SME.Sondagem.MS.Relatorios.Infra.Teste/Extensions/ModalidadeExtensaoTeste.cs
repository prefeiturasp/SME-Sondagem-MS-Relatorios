using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class ModalidadeExtensaoTeste
{
    [Theory]
    [InlineData(Modalidade.EJA)]
    [InlineData(Modalidade.CELP)]
    public void EhSemestral_DeveRetornarVerdadeiro_QuandoModalidadeSemestral(Modalidade modalidade)
    {
        var resultado = modalidade.EhSemestral();

        resultado.Should().BeTrue();
    }

    [Theory]
    [InlineData(Modalidade.Fundamental)]
    [InlineData(Modalidade.Medio)]
    [InlineData(Modalidade.Infantil)]
    [InlineData(Modalidade.CMCT)]
    [InlineData(Modalidade.MOVA)]
    [InlineData(Modalidade.ETEC)]
    [InlineData(Modalidade.CIEJA)]
    public void EhSemestral_DeveRetornarFalso_QuandoModalidadeNaoSemestral(Modalidade modalidade)
    {
        var resultado = modalidade.EhSemestral();

        resultado.Should().BeFalse();
    }

    [Fact]
    public void EhCelp_DeveRetornarVerdadeiro_QuandoModalidadeCelp()
    {
        var resultado = Modalidade.CELP.EhCelp();

        resultado.Should().BeTrue();
    }

    [Theory]
    [InlineData(Modalidade.EJA)]
    [InlineData(Modalidade.Fundamental)]
    [InlineData(Modalidade.Medio)]
    [InlineData(Modalidade.Infantil)]
    public void EhCelp_DeveRetornarFalso_QuandoModalidadeNaoCelp(Modalidade modalidade)
    {
        var resultado = modalidade.EhCelp();

        resultado.Should().BeFalse();
    }

    [Theory]
    [InlineData(Modalidade.Infantil, ModalidadeTipoCalendario.Infantil)]
    [InlineData(Modalidade.EJA, ModalidadeTipoCalendario.EJA)]
    [InlineData(Modalidade.CELP, ModalidadeTipoCalendario.CELP)]
    [InlineData(Modalidade.Fundamental, ModalidadeTipoCalendario.FundamentalMedio)]
    [InlineData(Modalidade.Medio, ModalidadeTipoCalendario.FundamentalMedio)]
    [InlineData(Modalidade.CIEJA, ModalidadeTipoCalendario.FundamentalMedio)]
    public void ObterModalidadeTipoCalendario_DeveRetornarTipoCorreto(Modalidade modalidade, ModalidadeTipoCalendario esperado)
    {
        var resultado = modalidade.ObterModalidadeTipoCalendario();

        resultado.Should().Be(esperado);
    }
}
