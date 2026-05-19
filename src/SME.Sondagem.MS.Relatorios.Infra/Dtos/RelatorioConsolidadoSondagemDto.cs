using System.Text.Json.Serialization;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;

namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class RelatorioConsolidadoSondagemDto
{
    public string Titulo { get; set; } = string.Empty;
    public IEnumerable<RelatorioConsolidadoQuestaoDto> Questoes { get; set; } = [];
    public Guid CodigoCorrelacao { get; set; }
    public int ProficienciaId { get; set; }

    public int SolicitacaoRelatorioId { get; set; }
    public string Agrupamento { get; set; } = string.Empty;
    public int AnoLetivo { get; set; }
    public string Modalidade { get; set; } = string.Empty;
    public string Dre { get; set; } = string.Empty;
    public string UnidadeEducacional { get; set; } = string.Empty;
    public string AnoTurma { get; set; } = string.Empty;
    public string ComponenteCurricular { get; set; } = string.Empty;
    public string Proficiencia { get; set; } = string.Empty;
    public string Bimestre { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Raca { get; set; } = string.Empty;
    public int ModalidadeId { get; set; }
    public int SemestreId { get; set; }
    public int? GeneroId { get; set; }
    public int? RacaId { get; set; }
    public bool? Pap { get; set; }
    public bool? Aee { get; set; }
    public bool? Deficiente { get; set; }
    public bool? PossuiLinguaPortuguesaSegundaLingua { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string UsuarioQueSolicitou { get; set; } = string.Empty;
    public DateTime DataImpressao { get; set; } = DateTime.Now;
    public DateTime? DataUltimaConsolidacao { get; set; }

    [JsonIgnore]
    public IEnumerable<RacaCor>? RacasDisponiveis { get; set; }

    [JsonIgnore]
    public IEnumerable<GeneroSexo>? GenerosDisponiveis { get; set; }

    [JsonIgnore]
    public IEnumerable<Dominio.Entidades.Bimestre>? BimestresDisponiveis { get; set; }
}

public class RelatorioConsolidadoQuestaoDto
{
    public int QuestaoId { get; set; }
    public string QuestaoNome { get; set; } = string.Empty;
    public IEnumerable<RelatorioConsolidadoRespostaDto>? Respostas { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoGeneroDto>? TotaisPorGenero { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoRacaDto>? TotaisPorRaca { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoGeneroRacaDto>? TotaisPorGeneroComRacas { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoAnoTurmaDto>? TotaisPorAnoTurma { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoBimestreDto>? TotaisPorBimestre { get; set; }

    public int TotalEstudantes { get; set; }
    public double PercentualTotal { get; set; }
}

public class RelatorioConsolidadoRespostaDto
{
    public string Resposta { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoRacaDto>? Racas { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoGeneroDto>? Generos { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoGeneroRacaDto>? GenerosComRacas { get; set; }


    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoAnoTurmaDto>? AnosTurma { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<RelatorioConsolidadoBimestreDto>? Bimestres { get; set; }

    public int Total { get; set; }
    public double Percentual { get; set; }
    public int Ordem { get; set; }
    public string? CorFundo { get; set; }
    public string? CorTexto { get; set; }
}

public class RelatorioConsolidadoRacaDto
{
    public string Raca { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public double Percentual { get; set; }
}

public class RelatorioConsolidadoGeneroDto
{
    public string? Genero { get; set; }
    public string? Sigla { get; set; }
    public int Quantidade { get; set; }
    public double Percentual { get; set; }
}

public class RelatorioConsolidadoGeneroRacaDto
{
    public string Genero { get; set; } = string.Empty;
    public int TotalGenero { get; set; }
    public double PercentualGenero { get; set; }
    public IEnumerable<RelatorioConsolidadoRacaDto>? Racas { get; set; }
}

public class RelatorioConsolidadoAnoTurmaDto
{
    public int AnoTurma { get; set; }
    public int Quantidade { get; set; }
    public double Percentual { get; set; }
}

public class RelatorioConsolidadoBimestreDto
{
    public string Bimestre { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public double Percentual { get; set; }
}

