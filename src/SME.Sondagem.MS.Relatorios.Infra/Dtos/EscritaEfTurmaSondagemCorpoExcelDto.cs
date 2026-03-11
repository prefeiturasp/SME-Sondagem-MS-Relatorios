namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class EscritaEfTurmaSondagemCorpoExcelDto
{
    public string? Numero { get; set; }
    public string? Nome { get; set; }
    public string? Raca { get; set; }
    public string? Genero { get; set; }
    public string? LpComoLinguaPrincipal { get; set; }
    public string? SondagemInicial { get; set; }
    public string? PrimeiroBimestre { get; set; }
    public string? SegundoBimestre { get; set; }
    public string? TerceiroBimestre { get; set; }
    public string? QuartoBimestre { get; set; }
    public string? Cor { get; set; }
    public bool Pap { get; set; }
    public bool Aee { get; set; }
    public bool PossuiDeficiencia { get; set; }
}