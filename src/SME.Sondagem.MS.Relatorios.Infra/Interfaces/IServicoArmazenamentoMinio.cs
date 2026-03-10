namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IServicoArmazenamentoMinio
{
    Task<string> UploadRelatorioAsync(byte[] arquivo, string nomeArquivo, string contentType = "application/pdf");
    Task<string> GerarLinkDownloadAsync(string nomeArquivo, int minutosValidade = 1440); //24 horas
}
