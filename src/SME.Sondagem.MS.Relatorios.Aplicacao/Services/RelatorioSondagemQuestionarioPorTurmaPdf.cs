using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Services;

public class RelatorioSondagemQuestionarioPorTurmaPdf : IRelatorioSondagemQuestionarioPorTurmaPdf
{
    private readonly IReportConverter reportConverter;
    //private readonly IHtmlHelper htmlHelper;

    public RelatorioSondagemQuestionarioPorTurmaPdf(IReportConverter reportConverter)
    {
        this.reportConverter = reportConverter;
    }

    public async Task<bool> GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(ConsultaSondagemPorTurmaDto consultaSondagemPorTurmaDto, Guid codigoCorrelacao)
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var nomeDiretorio = Path.Combine(directory, "relatorios");

        if (!Directory.Exists(nomeDiretorio))
            Directory.CreateDirectory(nomeDiretorio);

        var nomeArquivo = Path.Combine(nomeDiretorio, $"{codigoCorrelacao}.pdf");

        var relatorioHtml = RelatorioSondagemQuestionarioPorTurmaTemplate.GerarHtml(consultaSondagemPorTurmaDto);


        reportConverter.Converter(relatorioHtml, nomeArquivo);


        //await File.WriteAllTextAsync(headerPath, headerHtml);
        //await File.WriteAllTextAsync(footerPath, footerHtml);

        //converter.Convert(doc);

        //if (File.Exists(headerPath)) File.Delete(headerPath);
        //if (File.Exists(footerPath)) File.Delete(footerPath);

        //await servicoFila.PublicaFila(new PublicaFilaDto(
        //       new MensagemRelatorioProntoDto(request.MensagemUsuario, string.Empty),
        //       RotasRabbitSGP.RotaRelatoriosProntosSgp,
        //       ExchangeRabbit.Sgp,
        //       request.CodigoCorrelacao));

        return true;
    }
}
