using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using WkHtmlToPdfDotNet;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;

public interface IReportConverter
{
    byte[] ConvertToPdf(List<string> paginas);
    void Converter(string html, string nomeArquivo, string tituloRelatorioRodape = "", EnumTipoDePaginacao tipoDePaginacao = EnumTipoDePaginacao.PaginaComTotalPaginas, string templateHeader = "");
    void ConvertToPdfPaginacaoSolo(List<PaginaParaRelatorioPaginacaoSoloDto> paginas, string caminhoBase, string nomeArquivo, string tituloRelatorioRodape = "", Orientation orientacaoRelatorio = Orientation.Portrait);
    byte[] ConvertHtmlToPdfLandscape(string html, string caminhoBase, string nomeArquivo);
}
