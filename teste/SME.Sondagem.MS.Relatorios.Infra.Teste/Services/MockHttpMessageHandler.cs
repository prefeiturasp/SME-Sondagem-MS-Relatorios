using System.Net;
using System.Net.Http;
using System.Text;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

/// <summary>
/// Fake HttpMessageHandler that returns a predetermined response for unit tests.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    /// <summary>Última URI solicitada (útil para asserts em testes de cliente HTTP).</summary>
    public Uri? UltimaRequisicaoUri { get; private set; }

    public MockHttpMessageHandler(HttpStatusCode statusCode, string content = "")
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UltimaRequisicaoUri = request.RequestUri;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
