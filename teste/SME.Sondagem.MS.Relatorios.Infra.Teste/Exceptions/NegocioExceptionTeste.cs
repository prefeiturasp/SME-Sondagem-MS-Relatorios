using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Infra.Exceptions;
using System.Net;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Exceptions;

public class NegocioExceptionTeste
{
    [Fact]
    public void Construtor_DeveDefinirMensagemEStatusCodePadrao_QuandoCriadoComMensagemApenas()
    {
        const string mensagem = "Erro de negócio";

        var excecao = new NegocioException(mensagem);

        excecao.Message.Should().Be(mensagem);
        excecao.StatusCode.Should().Be(409);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(422)]
    [InlineData(500)]
    public void Construtor_DeveDefinirStatusCodeInformado_QuandoCriadoComStatusCodeInt(int statusCodeEsperado)
    {
        var excecao = new NegocioException("Erro", statusCodeEsperado);

        excecao.StatusCode.Should().Be(statusCodeEsperado);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, 400)]
    [InlineData(HttpStatusCode.NotFound, 404)]
    [InlineData(HttpStatusCode.InternalServerError, 500)]
    [InlineData(HttpStatusCode.Forbidden, 403)]
    public void Construtor_DeveConverterHttpStatusCode_QuandoCriadoComHttpStatusCode(HttpStatusCode httpStatusCode, int codigoEsperado)
    {
        var excecao = new NegocioException("Erro", httpStatusCode);

        excecao.StatusCode.Should().Be(codigoEsperado);
    }

    [Fact]
    public void NegocioException_DeveHerdarDeException()
    {
        var excecao = new NegocioException("Erro");

        excecao.Should().BeAssignableTo<Exception>();
    }
}
