using SME.Sondagem.MS.Relatorios.Infra.Fila;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoMensageria
{
    Task<bool> Publicar(MensagemRabbit mensagemRabbit, string rota, string exchange, string nomeAcao);
}
