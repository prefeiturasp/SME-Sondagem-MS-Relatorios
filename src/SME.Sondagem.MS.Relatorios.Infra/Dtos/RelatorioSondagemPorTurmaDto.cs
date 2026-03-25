using SME.Sondagem.MS.Relatorios.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class RelatorioSondagemPorTurmaDto
{
    public string TituloTabelaRespostas { get; set; } = string.Empty;
    public int? Semestre { get; set; }
    public int? Bimestre { get; set; }
    public int AnoLetivo { get; set; }
    public string? Dre { get; set; } = string.Empty;
    public string? SiglaDre { get; set; } = string.Empty;
    public string Turma { get; set; } = string.Empty;
    public string UnidadeEducacional { get; set; } = string.Empty;
    public Modalidade Modalidade { get; set; }
    public string Proficiencia { get; set; } = string.Empty;
    public DateTime DataImpressao { get; set; } = DateTime.Now;
    public string Usuario { get; set; } = string.Empty;
    public List<EstudanteDto> Estudantes { get; set; } = [];
    public bool ExibeColunaLinguaPortuguesaSegundaLingua { get; set; }
    public Guid CodigoCorrelacao { get; set; }
}