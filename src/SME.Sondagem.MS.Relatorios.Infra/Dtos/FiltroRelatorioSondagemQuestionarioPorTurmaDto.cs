namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class FiltroRelatorioSondagemQuestionarioPorTurmaDto
{
    public int TurmaId { get; set; }
    public int ProficienciaId { get; set; }
    public int ComponenteCurricularId { get; set; }
    public int Modalidade { get; set; }
    public int Ano { get; set; }
    public int AnoLetivo { get; set; }
    public int SemestreId { get; set; }
    public string UeCodigo { get; set; } = string.Empty;
    public int? BimestreId { get; set; }
    public int ExtensaoRelatorio { get; set; }
    public int SolicitacaoRelatorioId { get; set; }
    public int TipoRelatorio { get; set; }
    public int StatusSolicitacao { get; set; }
    public string UsuarioQueSolicitou { get; set; } = string.Empty;
}
