namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class RelatorioSondagemQuestionarioHtmlParaPdfDto
{
    public Guid CodigoCorrelacao { get; set; }
    public string NomeTemplate { get; set; }
    public List<RelatorioSondagemPorTurmaDto> Paginas { get; set; }
    public string MensagemUsuario { get; set; }
}
