namespace SME.Sondagem.MS.Relatorios.Dominio.Entidades;

public class ComponenteCurricular : EntidadeBase
{
    public string? Nome { get;  set; }
    public int? Ano { get;  set; }
    public string? Modalidade { get;  set; }
    public int CodigoEol { get;  set; }
}
