namespace SME.Sondagem.MS.Relatorios.Dominio.Entidades;

public class Bimestre : EntidadeBase
{
    public Bimestre(int codBimestreEnsinoEol, string descricao)
    {
        CodBimestreEnsinoEol = codBimestreEnsinoEol;
        Descricao = descricao;
    }

    public int CodBimestreEnsinoEol { get; private set; }
    public string Descricao { get; private set; }
}
