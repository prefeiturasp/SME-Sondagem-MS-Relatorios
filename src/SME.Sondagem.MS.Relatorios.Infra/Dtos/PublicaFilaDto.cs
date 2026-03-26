namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class PublicaFilaDto
{
    public PublicaFilaDto(object dados, string rota, string exchange = "", Guid codigoCorrelacao = default, string codigoRfUsuario = "")
    {
        Dados = dados;

        Rota = rota;
        if (!string.IsNullOrWhiteSpace(exchange))
            Exchange = exchange;
        CodigoCorrelacao = codigoCorrelacao;
        UsuarioLogadoRF = codigoRfUsuario;
    }

    public string? NomeFila { get; set; }
    public object Dados { get; set; }
    public string? Rota { get; }
    public string? Exchange { get; set; }
    public string? UsuarioLogadoRF { get; }

    public Guid CodigoCorrelacao { get; set; }
}