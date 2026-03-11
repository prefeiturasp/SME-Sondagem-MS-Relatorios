namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class ConsultaSondagemPorTurmaDto
{
    public string TituloTabelaRespostas { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public string? Dre { get; set; } = string.Empty;
    public string? SiglaDre { get; set; } = string.Empty;
    public string Turma { get; set; } = string.Empty;
    public string UnidadeEducacional { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public string Proficiencia { get; set; } = string.Empty;
    public DateTime DataImpressao { get; set; } = DateTime.Now;
    public string Usuario { get; set; } = string.Empty;
    public List<EstudanteDto> Estudantes { get; set; } = [];
}

public class EstudanteDto
{
    public string NumeroAlunoChamada { get; set; }
    public bool LinguaPortuguesaSegundaLingua { get; set; }
    public long Codigo { get; set; }
    public string Raca { get; set; }
    public string Genero { get; set; }
    public string NomeRelatorio { get; set; }
    public bool Pap { get; set; }
    public bool Aee { get; set; }
    public bool PossuiDeficiencia { get; set; }
    public List<ColunaDto> Coluna { get; set; } = [];
}

public class ColunaDto
{
    public int IdCiclo { get; set; }
    public string DescricaoColuna { get; set; }
    public bool PeriodoBimestreAtivo { get; set; }
    public int? QuestaoSubrespostaId { get; set; }
    public List<OpcaoRespostaDto> OpcaoResposta { get; set; } = [];
    public RespostaDto Resposta { get; set; }
}

public class OpcaoRespostaDto
{
    public int Id { get; set; }
    public int Ordem { get; set; }
    public string DescricaoOpcaoResposta { get; set; }
    public string Legenda { get; set; }
    public string CorFundo { get; set; }
    public string CorTexto { get; set; }
}

public class RespostaDto
{
    public int Id { get; set; }
    public int? OpcaoRespostaId { get; set; }
}