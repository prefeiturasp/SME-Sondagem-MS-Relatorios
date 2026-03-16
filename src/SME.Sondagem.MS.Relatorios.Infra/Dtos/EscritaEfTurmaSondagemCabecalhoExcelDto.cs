namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class EscritaEfTurmaSondagemCabecalhoExcelDto
{
    public EscritaEfTurmaSondagemCabecalhoExcelDto()
    {
        CorpoRelatorio = new List<EscritaEfTurmaSondagemCorpoExcelDto>();
    }
    public int AnoLetivo { get; set; }
    public string? SemestreId { get; set; }
    public string? BimestreId { get; set; }
    public string? Turma { get; set; }
    public string? Ue { get; set; }
    public string? Dre { get; set; }
    public string? Modalidade { get; set; }
    public string? Proeficiencia { get; set; }
    public string? DataImpressao { get; set; }
    public string? NomeUsuarioSolicitacao { get; set; }
    public List<EscritaEfTurmaSondagemCorpoExcelDto> CorpoRelatorio { get; set; }
    public bool ExibeColunaLinguaPortuguesaSegundaLingua { get; set; }
}
