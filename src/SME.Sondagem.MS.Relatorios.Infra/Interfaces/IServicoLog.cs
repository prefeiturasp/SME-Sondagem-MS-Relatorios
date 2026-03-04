using SME.Sondagem.MS.Relatorios.Infra.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoLog
{
    void Registrar(Exception ex);
    void Registrar(string mensagem, Exception ex);
    void Registrar(LogNivel nivel, string erro, string observacoes, string stackTrace);
}