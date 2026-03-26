namespace SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;

public class OpcaoRespostaDto
{
    public int Id { get; set; }
    public int Ordem { get; set; }
    public string DescricaoOpcaoResposta { get; set; } = string.Empty;
    public string? Legenda { get; set; }
    public string? CorFundo { get; set; }
    public string? CorTexto { get; set; }
}
