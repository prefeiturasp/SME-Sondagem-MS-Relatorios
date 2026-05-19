namespace SME.Sondagem.MS.Relatorios.Dominio.Entidades;

public abstract class EntidadeBase
{
    protected EntidadeBase()
    {
    }

    public int Id { get; set; }
    public DateTime? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
    public string? AlteradoRF { get; set; }
    public DateTime CriadoEm { get; set; }
    public string CriadoPor { get; set; } = string.Empty;
    public string CriadoRF { get; set; } = string.Empty;
    public bool Excluido { get; set; } = false;
}
