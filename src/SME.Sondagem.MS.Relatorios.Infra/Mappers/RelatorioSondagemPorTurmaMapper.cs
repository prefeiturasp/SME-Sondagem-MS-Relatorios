using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Records;

namespace SME.Sondagem.MS.Relatorios.Infra.Mappers;

public static class RelatorioSondagemPorTurmaMapper
{
    public static RelatorioSondagemPorTurmaDto ParaDto(this RetornoApiSondagemQuestionarioDto source, 
                                                      EscolaDto? escolaDto, 
                                                      TurmaDto turmaDto, 
                                                      DadosUsuarioDto dadosUsuarioDto,
                                                      Modalidade modalidade,
                                                      bool exibeColunaLinguaPortuguesaSegundaLingua)
    {
        if (source == null) return new RelatorioSondagemPorTurmaDto();

        return new RelatorioSondagemPorTurmaDto
        {
            AnoLetivo = turmaDto.AnoLetivo,
            Dre = escolaDto?.NomeDRE,
            SiglaDre = escolaDto?.SiglaDRE,
            Turma = $"{modalidade.ShortName()} - {turmaDto.NomeTurma} - {turmaDto.Ano}° ANO",
            UnidadeEducacional = $"{escolaDto?.CodigoEscola} - {escolaDto?.SiglaTipoEscola} - {escolaDto?.NomeEscola}",
            Proficiencia = source.TituloTabelaRespostas,
            Modalidade = modalidade,
            TituloTabelaRespostas = source.TituloTabelaRespostas,
            Semestre = source.SemestreId,
            Bimestre = source.BimestreId,
            Usuario = $"{dadosUsuarioDto.Nome} ({dadosUsuarioDto.CodigoRf})",
            Estudantes = source?.Estudantes?.Select(e => e.ParaDto())?.ToList() ?? [],
            ExibeColunaLinguaPortuguesaSegundaLingua = exibeColunaLinguaPortuguesaSegundaLingua
        };
    }

    private static EstudanteDto ParaDto(this Estudante source)
    {
        if (source == null) return new EstudanteDto();

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
            Coluna = source?.Colunas?.Select(c => c.ParaDto())?.ToList() ?? []
        };
    }

    private static ColunaQuestionarioDto ParaDto(this Coluna source)
    {
        if (source == null) return new ColunaQuestionarioDto();

        return new ColunaQuestionarioDto
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
        if (source == null) return new OpcaoRespostaDto();

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
        if (source == null) return new RespostaDto();

        return new RespostaDto
        {
            Id = source.Id,
            OpcaoRespostaId = source.OpcaoRespostaId
        };
    }
}