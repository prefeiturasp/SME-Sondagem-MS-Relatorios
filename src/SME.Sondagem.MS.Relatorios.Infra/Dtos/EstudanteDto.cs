using SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;

namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class EstudanteDto
{
    public string? NumeroAlunoChamada { get; set; }
    public bool LinguaPortuguesaSegundaLingua { get; set; }
    public long Codigo { get; set; }
    public string? Raca { get; set; }
    public string? Genero { get; set; }
    public string? NomeRelatorio { get; set; }
    public bool Pap { get; set; }
    public bool Aee { get; set; }
    public bool PossuiDeficiencia { get; set; }
    public List<ColunaQuestionarioDto> Coluna { get; set; } = [];
}
