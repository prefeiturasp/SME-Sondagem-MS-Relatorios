using SME.Sondagem.MS.Relatorios.Infra.Extensions;

namespace SME.Sondagem.MS.Relatorios.Infra.Fila;

public class MensagemRabbit
{
    public MensagemRabbit(object? mensagem, Guid codigoCorrelacao)
    {
        Mensagem = mensagem;
        CodigoCorrelacao = codigoCorrelacao;
    }

    public object? Mensagem { get; set; }
    public Guid CodigoCorrelacao { get; set; }
    public T? ObterObjetoMensagem<T>() where T : class
    {
        return Mensagem?.ToString().ConverterObjectStringPraObjeto<T>();
    }
}
