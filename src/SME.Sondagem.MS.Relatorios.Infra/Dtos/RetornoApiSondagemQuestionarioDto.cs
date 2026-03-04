namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class RetornoApiSondagemQuestionarioDto
{
    public string TituloTabelaRespostas { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public IEnumerable<EstudanteApiSondagemDto>? Estudantes { get; set; }
    public IEnumerable<LegendaApiSondagemDto>? Legenda { get; set; }
}
