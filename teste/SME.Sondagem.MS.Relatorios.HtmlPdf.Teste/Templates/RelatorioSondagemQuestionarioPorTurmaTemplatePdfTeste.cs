using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;
using System;
using System.Collections.Generic;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Teste.Templates;

public class RelatorioSondagemQuestionarioPorTurmaTemplatePdfTeste
{
    private readonly RelatorioSondagemQuestionarioPorTurmaTemplatePdf _templatePdf;

    public RelatorioSondagemQuestionarioPorTurmaTemplatePdfTeste()
    {
        _templatePdf = new RelatorioSondagemQuestionarioPorTurmaTemplatePdf();
    }

    [Fact]
    public void GerarHtml_DeveRetornarHtmlValido_QuandoDtoForPreenchido()
    {
        // Arrange
        var dto = new RelatorioSondagemPorTurmaDto
        {
            AnoLetivo = 2023,
            Modalidade = Modalidade.EJA,
            SiglaDre = "DRE-IT",
            UnidadeEducacional = "EMEF TESTE",
            Turma = "1A",
            Proficiencia = "Leitura",
            Bimestre = 1,
            Semestre = 1,
            Usuario = "admin",
            DataImpressao = new DateTime(2023, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            TituloTabelaRespostas = "Sondagem de Leitura",
            ExibeColunaLinguaPortuguesaSegundaLingua = true,
            Estudantes = new List<EstudanteDto>
            {
                new EstudanteDto
                {
                    NumeroAlunoChamada = "01",
                    NomeRelatorio = "João Silva",
                    Raca = "Parda",
                    Genero = "Masculino",
                    LinguaPortuguesaSegundaLingua = true,
                    Aee = true,
                    Pap = false,
                    PossuiDeficiencia = false,
                    Coluna = new List<ColunaQuestionarioDto>
                    {
                        new ColunaQuestionarioDto
                        {
                            IdCiclo = 1,
                            QuestaoSubrespostaId = 1,
                            DescricaoColuna = "Pergunta 1",
                            Resposta = new RespostaDto { OpcaoRespostaId = 1 },
                            OpcaoResposta = new List<OpcaoRespostaDto>
                            {
                                new OpcaoRespostaDto { Id = 1, DescricaoOpcaoResposta = "Sim", CorFundo = "#000000", CorTexto = "#ffffff" }
                            }
                        }
                    }
                }
            }
        };

        // Act
        var html = _templatePdf.GerarHtml(dto);

        // Assert
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("Sondagem de Leitura");
        html.Should().Contain("João Silva");
        html.Should().Contain("DRE-IT");
        html.Should().Contain("EMEF TESTE");
        html.Should().Contain("1A");
        html.Should().Contain("Masculino");
        html.Should().Contain("Parda");
        html.Should().Contain("Semestre:");
        html.Should().Contain("1");
        html.Should().Contain("Sim"); // Opção de resposta
    }

    [Fact]
    public void GerarHtml_DeveTratarEstudantesSemResposta()
    {
        // Arrange
        var dto = new RelatorioSondagemPorTurmaDto
        {
            AnoLetivo = 2023,
            Bimestre = 1,
            Semestre = 1,
            TituloTabelaRespostas = "Sondagem sem resposta",
            Estudantes = new List<EstudanteDto>
            {
                new EstudanteDto
                {
                    NumeroAlunoChamada = "02",
                    NomeRelatorio = "Maria Souza",
                    Raca = "Branca",
                    Genero = "Feminino",
                    Coluna = new List<ColunaQuestionarioDto>
                    {
                        new ColunaQuestionarioDto
                        {
                            IdCiclo = 1,
                            QuestaoSubrespostaId = 1,
                            DescricaoColuna = "Pergunta 1",
                            Resposta = null // Sem resposta
                        }
                    }
                }
            }
        };

        // Act
        var html = _templatePdf.GerarHtml(dto);

        // Assert
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain("Maria Souza");
        html.Should().Contain("class=\"resposta-vazio\"");
        html.Should().Contain("-"); // Sinal de vazio conforme GerarTdResposta
    }
    
    [Fact]
    public void GerarGrafico_DeveRetornarVazio_QuandoNaoHouverBarras()
    {
        // Arrange
        var dto = new GraficoSondagemDto
        {
            Barras = new List<GraficoBarraDto>()
        };

        // Act
        var grafico = RelatorioSondagemQuestionarioPorTurmaTemplatePdf.GerarGrafico(dto);

        // Assert
        grafico.Should().BeEmpty();
    }
}
