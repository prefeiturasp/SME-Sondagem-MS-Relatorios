namespace SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;

public class ColunaQuestionarioDto
{
    public int IdCiclo { get; set; }
    public string DescricaoColuna { get; set; } = string.Empty;
    public bool PeriodoBimestreAtivo { get; set; }
    public int? QuestaoSubrespostaId { get; set; }
    public IEnumerable<OpcaoRespostaDto>? OpcaoResposta { get; set; }
    public RespostaDto? Resposta { get; set; }
}
