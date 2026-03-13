namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class ParametroSondagemDto
{
    public int Id { get; set; }
    public int IdQuestionario { get; set; }
    public string? Valor { get; set; }
    public string? Tipo { get; set; }
}
