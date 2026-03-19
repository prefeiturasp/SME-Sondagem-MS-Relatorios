using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Extensions;

public class SerializerExtensionsTeste
{
    private record PessoaStub(string Nome, int Idade);

    [Fact]
    public void ConverterObjectParaJson_DeveSerializarObjeto_QuandoObjetoValido()
    {
        var objeto = new PessoaStub("Carlos", 30);

        var json = objeto.ConverterObjectParaJson();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Carlos");
        json.Should().Contain("30");
    }

    [Fact]
    public void ConverterObjectParaJson_DeveRetornarStringVazia_QuandoObjetoNulo()
    {
        object? objeto = null;

        var json = objeto!.ConverterObjectParaJson();

        json.Should().BeEmpty();
    }

    [Fact]
    public void ConverterObjectStringPraObjeto_DeveDesserializarJson_QuandoJsonValido()
    {
        var json = "{\"Nome\":\"Maria\",\"Idade\":25}";

        var resultado = json.ConverterObjectStringPraObjeto<PessoaStub>();

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Maria");
        resultado.Idade.Should().Be(25);
    }

    [Fact]
    public void ConverterObjectStringPraObjeto_DeveRetornarDefault_QuandoJsonNulo()
    {
        string json = null!;

        var resultado = json.ConverterObjectStringPraObjeto<PessoaStub>();

        resultado.Should().BeNull();
    }

    [Fact]
    public void ConverterObjectStringPraObjeto_DeveRetornarDefault_QuandoJsonVazio()
    {
        var json = string.Empty;

        var resultado = json.ConverterObjectStringPraObjeto<PessoaStub>();

        resultado.Should().BeNull();
    }

    [Fact]
    public void ConverterObjectParaJson_EConverterObjectStringPraObjeto_DevemSerSimetricos()
    {
        var original = new PessoaStub("Ana", 40);

        var json = original.ConverterObjectParaJson();
        var restaurado = json.ConverterObjectStringPraObjeto<PessoaStub>();

        restaurado.Should().BeEquivalentTo(original);
    }
}
