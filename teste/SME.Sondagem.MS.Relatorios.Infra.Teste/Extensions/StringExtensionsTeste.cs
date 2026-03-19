using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class StringExtensionsTeste
{
    [Theory]
    [InlineData("10", 10)]
    [InlineData("0", 0)]
    [InlineData("-5", -5)]
    [InlineData("2147483647", int.MaxValue)]
    public void ConverterParaInt_DeveRetornarInteiro_QuandoStringValida(string valor, int esperado)
    {
        var resultado = valor.ConverterParaInt();

        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("1.5")]
    [InlineData(null)]
    public void ConverterParaInt_DeveRetornarZero_QuandoStringInvalida(string? valor)
    {
        var resultado = valor!.ConverterParaInt();

        resultado.Should().Be(0);
    }
}
