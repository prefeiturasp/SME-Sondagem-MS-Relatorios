namespace SME.Sondagem.MS.Relatorios.Infra.Dtos;

public class FinalizarSolicitacaoRelatorioDto
{
    public FinalizarSolicitacaoRelatorioDto(int solicitacaoRelatorioId, string urlRelatorio, Guid codigoCorrelacao)
    {
        SolicitacaoRelatorioId = solicitacaoRelatorioId;
        UrlRelatorio = urlRelatorio;
        CodigoCorrelacao = codigoCorrelacao;
    }

    public int SolicitacaoRelatorioId { get; set; }
    public string UrlRelatorio { get; set; }
    public Guid CodigoCorrelacao { get; set; }
}
