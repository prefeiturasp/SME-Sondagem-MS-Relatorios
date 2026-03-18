using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Exceptions;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class ModalidadeTipoCalendarioExtensionTeste
{
    [Fact]
    public void ObterModalidades_DeveRetornarFundamentalEMedio_QuandoFundamentalMedio()
    {
        var resultado = ModalidadeTipoCalendario.FundamentalMedio.ObterModalidades();

        resultado.Should().BeEquivalentTo(new[] { Modalidade.Fundamental, Modalidade.Medio });
    }

    [Fact]
    public void ObterModalidades_DeveRetornarEja_QuandoEja()
    {
        var resultado = ModalidadeTipoCalendario.EJA.ObterModalidades();

        resultado.Should().BeEquivalentTo(new[] { Modalidade.EJA });
    }

    [Fact]
    public void ObterModalidades_DeveRetornarInfantil_QuandoInfantil()
    {
        var resultado = ModalidadeTipoCalendario.Infantil.ObterModalidades();

        resultado.Should().BeEquivalentTo(new[] { Modalidade.Infantil });
    }

    [Fact]
    public void ObterModalidades_DeveRetornarCelp_QuandoCelp()
    {
        var resultado = ModalidadeTipoCalendario.CELP.ObterModalidades();

        resultado.Should().BeEquivalentTo(new[] { Modalidade.CELP });
    }

    [Fact]
    public void ObterModalidades_DeveLancarNegocioException_QuandoModalidadeInvalida()
    {
        var modalidadeInvalida = (ModalidadeTipoCalendario)99;

        var acao = () => modalidadeInvalida.ObterModalidades();

        acao.Should().Throw<NegocioException>()
            .WithMessage("*Modalidade de tipo de calendário não identificado*");
    }
}
