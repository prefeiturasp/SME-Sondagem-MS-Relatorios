using System.Text.Json.Serialization;

namespace SME.Sondagem.MS.Relatorios.Infra.Records;

public record RetornoApiSondagemQuestionarioDto
(
    [property: JsonPropertyName("tituloTabelaRespostas")] string TituloTabelaRespostas,
    [property: JsonPropertyName("semestre")] string Semestre,
    [property: JsonPropertyName("estudantes")] List<Estudante> Estudantes,
    [property: JsonPropertyName("legenda")] List<Legenda> Legendas
);

public record Estudante(
    [property: JsonPropertyName("numeroAlunoChamada")] string NumeroAlunoChamada,
    [property: JsonPropertyName("linguaPortuguesaSegundaLingua")] bool LinguaPortuguesaSegundaLingua,
    [property: JsonPropertyName("codigo")] int Codigo,
    [property: JsonPropertyName("raca")] string Raca,
    [property: JsonPropertyName("genero")] string Genero,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("nomeRelatorio")] string NomeRelatorio,
    [property: JsonPropertyName("pap")] bool Pap,
    [property: JsonPropertyName("aee")] bool Aee,
    [property: JsonPropertyName("possuiDeficiencia")] bool PossuiDeficiencia,
    [property: JsonPropertyName("coluna")] List<Coluna> Colunas
);

public record Coluna(
    [property: JsonPropertyName("idCiclo")] int IdCiclo,
    [property: JsonPropertyName("descricaoColuna")] string DescricaoColuna,
    [property: JsonPropertyName("periodoBimestreAtivo")] bool PeriodoBimestreAtivo,
    [property: JsonPropertyName("questaoSubrespostaId")] int? QuestaoSubrespostaId,
    [property: JsonPropertyName("opcaoResposta")] List<OpcaoResposta> OpcoesResposta,
    [property: JsonPropertyName("resposta")] Resposta Resposta
);

public record OpcaoResposta(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("ordem")] int Ordem,
    [property: JsonPropertyName("descricaoOpcaoResposta")] string DescricaoOpcaoResposta,
    [property: JsonPropertyName("legenda")] string Legenda,
    [property: JsonPropertyName("corFundo")] string CorFundo,
    [property: JsonPropertyName("corTexto")] string CorTexto
);

public record Resposta(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("opcaoRespostaId")] int? OpcaoRespostaId
);

public record Legenda(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("ordem")] int Ordem,
    [property: JsonPropertyName("descricaoOpcaoResposta")] string DescricaoOpcaoResposta,
    [property: JsonPropertyName("legenda")] string TextoLegenda,
    [property: JsonPropertyName("corFundo")] string CorFundo,
    [property: JsonPropertyName("corTexto")] string CorTexto
);