using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Records;

namespace SME.Sondagem.MS.Relatorios.Infra.Mappers;

public static class ConsultaSondagemMapper
{
    public static ConsultaSondagemPorTurmaDto ParaDto(this RetornoApiSondagemQuestionarioDto source, 
                                                      EscolaDto? escolaDto, 
                                                      TurmaDto turmaDto, 
                                                      DadosUsuarioDto dadosUsuarioDto,
                                                      Modalidade modalidade,
                                                      bool exibeColunaLinguaPortuguesaSegundaLingua,
                                                      int? bimestreId,
                                                      int? semestreId)
    {
        if (source == null) return new ConsultaSondagemPorTurmaDto();

        return new ConsultaSondagemPorTurmaDto
        {
            AnoLetivo = turmaDto.AnoLetivo,
            Dre = escolaDto?.NomeDRE,
            SiglaDre = escolaDto?.SiglaDRE,
            Turma = $"{modalidade.ShortName()} - {turmaDto.NomeTurma} - {turmaDto.Ano}° ANO",
            UnidadeEducacional = $"{escolaDto?.CodigoEscola} - {escolaDto?.SiglaTipoEscola} - {escolaDto?.NomeEscola}",
            Proficiencia = source.TituloTabelaRespostas,
            Modalidade = modalidade,
            TituloTabelaRespostas = source.TituloTabelaRespostas,
            Semestre = semestreId.ToString(),
            Bimestre = bimestreId?.ToString(),
            Usuario = $"{dadosUsuarioDto.Nome} ({dadosUsuarioDto.CodigoRf})",
            Estudantes = source?.Estudantes?.Select(e => e.ParaDto())?.ToList(),
            ExibeColunaLinguaPortuguesaSegundaLingua = exibeColunaLinguaPortuguesaSegundaLingua
        };
    }

    private static EstudanteDto ParaDto(this Estudante source)
    {
        if (source == null) return null;

        return new EstudanteDto
        {
            NumeroAlunoChamada = source.NumeroAlunoChamada,
            LinguaPortuguesaSegundaLingua = source.LinguaPortuguesaSegundaLingua,
            Codigo = source.Codigo,
            Raca = source.Raca,
            Genero = source.Genero,
            NomeRelatorio = source.NomeRelatorio,
            Pap = source.Pap,
            Aee = source.Aee,
            PossuiDeficiencia = source.PossuiDeficiencia,
            Coluna = source.Colunas?.Select(c => c.ParaDto()).ToList()
        };
    }

    private static ColunaDto ParaDto(this Coluna source)
    {
        if (source == null) return null;

        return new ColunaDto
        {
            IdCiclo = source.IdCiclo,
            DescricaoColuna = source.DescricaoColuna,
            PeriodoBimestreAtivo = source.PeriodoBimestreAtivo,
            QuestaoSubrespostaId = source.QuestaoSubrespostaId,
            OpcaoResposta = source.OpcoesResposta?.Select(o => o.ParaDto()).ToList(),
            Resposta = source.Resposta?.ParaDto()
        };
    }

    private static OpcaoRespostaDto ParaDto(this OpcaoResposta source)
    {
        if (source == null) return null;

        return new OpcaoRespostaDto
        {
            Id = source.Id,
            Ordem = source.Ordem,
            DescricaoOpcaoResposta = source.DescricaoOpcaoResposta,
            Legenda = source.Legenda,
            CorFundo = source.CorFundo,
            CorTexto = source.CorTexto
        };
    }

    private static RespostaDto ParaDto(this Resposta source)
    {
        if (source == null) return null;

        return new RespostaDto
        {
            Id = source.Id,
            OpcaoRespostaId = source.OpcaoRespostaId
        };
    }
}