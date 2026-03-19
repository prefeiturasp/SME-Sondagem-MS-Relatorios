using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Mappers;
using SME.Sondagem.MS.Relatorios.Infra.Records;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Mappers;

public class RelatorioSondagemPorTurmaMapperTeste
{
    private static TurmaDto CriarTurmaDto(int anoLetivo = 2024, string nomeTurma = "Turma A", char ano = '5') =>
        new()
        {
            AnoLetivo = anoLetivo,
            NomeTurma = nomeTurma,
            Ano = ano
        };

    private static EscolaDto CriarEscolaDto() =>
        new()
        {
            NomeDRE = "DRE Norte",
            SiglaDRE = "DN",
            CodigoEscola = "001",
            SiglaTipoEscola = "EMEF",
            NomeEscola = "Escola Modelo"
        };

    private static DadosUsuarioDto CriarDadosUsuarioDto() =>
        new()
        {
            Nome = "Professor Silva",
            CodigoRf = "123456"
        };

    private static RetornoApiSondagemQuestionarioDto CriarRetorno(string titulo = "Proficiência") =>
        new(titulo, "1", "2", new List<Estudante>(), new List<Legenda>(), 1);

    [Fact]
    public void ParaDto_DeveRetornarDtoVazio_QuandoSourceNulo()
    {
        RetornoApiSondagemQuestionarioDto? source = null;

        var resultado = source!.ParaDto(null, CriarTurmaDto(), CriarDadosUsuarioDto(), Modalidade.Fundamental, false);

        resultado.Should().NotBeNull();
        resultado.Turma.Should().BeEmpty();
        resultado.Estudantes.Should().BeEmpty();
    }

    [Fact]
    public void ParaDto_DeveMapearCamposBasicos_QuandoSourceValido()
    {
        var source = CriarRetorno("Meu Titulo");
        var escola = CriarEscolaDto();
        var turma = CriarTurmaDto(2024, "Turma B", '3');
        var usuario = CriarDadosUsuarioDto();

        var resultado = source.ParaDto(escola, turma, usuario, Modalidade.Fundamental, false);

        resultado.AnoLetivo.Should().Be(2024);
        resultado.Dre.Should().Be("DRE Norte");
        resultado.SiglaDre.Should().Be("DN");
        resultado.Proficiencia.Should().Be("Meu Titulo");
        resultado.TituloTabelaRespostas.Should().Be("Meu Titulo");
        resultado.Semestre.Should().Be("1");
        resultado.Bimestre.Should().Be("2");
        resultado.Modalidade.Should().Be(Modalidade.Fundamental);
        resultado.ExibeColunaLinguaPortuguesaSegundaLingua.Should().BeFalse();
    }

    [Fact]
    public void ParaDto_DeveFormatarTurmaCorretamente_QuandoFundamental()
    {
        var source = CriarRetorno();
        var turma = CriarTurmaDto(2024, "Turma C", '4');

        var resultado = source.ParaDto(null, turma, CriarDadosUsuarioDto(), Modalidade.Fundamental, false);

        resultado.Turma.Should().Contain("EF");
        resultado.Turma.Should().Contain("Turma C");
        resultado.Turma.Should().Contain("4° ANO");
    }

    [Fact]
    public void ParaDto_DeveFormatarUnidadeEducacionalCorretamente_QuandoEscolaNaoNula()
    {
        var source = CriarRetorno();
        var escola = CriarEscolaDto();

        var resultado = source.ParaDto(escola, CriarTurmaDto(), CriarDadosUsuarioDto(), Modalidade.Fundamental, false);

        resultado.UnidadeEducacional.Should().Contain("001");
        resultado.UnidadeEducacional.Should().Contain("EMEF");
        resultado.UnidadeEducacional.Should().Contain("Escola Modelo");
    }

    [Fact]
    public void ParaDto_DeveFormatarUsuarioCorretamente()
    {
        var source = CriarRetorno();
        var usuario = new DadosUsuarioDto { Nome = "Maria", CodigoRf = "654321" };

        var resultado = source.ParaDto(null, CriarTurmaDto(), usuario, Modalidade.Fundamental, false);

        resultado.Usuario.Should().Be("Maria (654321)");
    }

    [Fact]
    public void ParaDto_DeveMapearEstudantesCorretamente_QuandoExistemEstudantes()
    {
        var estudantes = new List<Estudante>
        {
            new("001", false, 1, "Branca", "M", "João", "João S.", false, false, false, new List<Coluna>()),
            new("002", true, 2, "Parda", "F", "Maria", "Maria S.", true, false, true, new List<Coluna>())
        };
        var source = new RetornoApiSondagemQuestionarioDto("Titulo", "1", "1", estudantes, new List<Legenda>(), 1);

        var resultado = source.ParaDto(null, CriarTurmaDto(), CriarDadosUsuarioDto(), Modalidade.Fundamental, false);

        resultado.Estudantes.Should().HaveCount(2);
        resultado.Estudantes![0].NumeroAlunoChamada.Should().Be("001");
        resultado.Estudantes[0].LinguaPortuguesaSegundaLingua.Should().BeFalse();
        resultado.Estudantes[1].NumeroAlunoChamada.Should().Be("002");
        resultado.Estudantes[1].LinguaPortuguesaSegundaLingua.Should().BeTrue();
        resultado.Estudantes[1].Pap.Should().BeTrue();
        resultado.Estudantes[1].PossuiDeficiencia.Should().BeTrue();
    }

    [Fact]
    public void ParaDto_DeveDefinirExibeColunaSegundaLinguaVerdadeiro_QuandoPassadoVerdadeiro()
    {
        var source = CriarRetorno();

        var resultado = source.ParaDto(null, CriarTurmaDto(), CriarDadosUsuarioDto(), Modalidade.Infantil, true);

        resultado.ExibeColunaLinguaPortuguesaSegundaLingua.Should().BeTrue();
    }

    [Fact]
    public void ParaDto_DeveMapearColunasDeEstudante_QuandoExistemColunas()
    {
        var colunas = new List<Coluna>
        {
            new(1, "Coluna 1", true, null,
                new List<OpcaoResposta> { new(1, 1, "Desc", "Leg", "Azul", "Branco") },
                new Resposta(10, 1))
        };
        var estudantes = new List<Estudante>
        {
            new("003", false, 3, "Amarela", "M", "Pedro", "Pedro S.", false, false, false, colunas)
        };
        var source = new RetornoApiSondagemQuestionarioDto("Titulo", "1", "1", estudantes, new List<Legenda>(), 1);

        var resultado = source.ParaDto(null, CriarTurmaDto(), CriarDadosUsuarioDto(), Modalidade.Fundamental, false);

        var estudanteResultado = resultado.Estudantes!.First();
        estudanteResultado.Coluna.Should().HaveCount(1);
        var colunaResultado = estudanteResultado.Coluna![0];
        colunaResultado.IdCiclo.Should().Be(1);
        colunaResultado.DescricaoColuna.Should().Be("Coluna 1");
        colunaResultado.PeriodoBimestreAtivo.Should().BeTrue();
        colunaResultado.Resposta!.Id.Should().Be(10);
        colunaResultado.Resposta.OpcaoRespostaId.Should().Be(1);
        colunaResultado.OpcaoResposta.Should().HaveCount(1);
        var opcaoResultado = colunaResultado.OpcaoResposta!.First();
        opcaoResultado.DescricaoOpcaoResposta.Should().Be("Desc");
        opcaoResultado.CorFundo.Should().Be("Azul");
    }
}
