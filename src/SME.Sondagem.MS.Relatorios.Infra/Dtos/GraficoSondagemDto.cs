namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class GraficoSondagemDto
{
    public string Titulo { get; set; }
    public string Subtitulo { get; set; }
    public List<GraficoBarraDto> Barras { get; set; } = new List<GraficoBarraDto>();
}

public class GraficoBarraDto
{
    public string Legenda { get; set; }
    public string CorFundo { get; set; }
    public string CorTexto { get; set; }
    public int Quantidade { get; set; }
}