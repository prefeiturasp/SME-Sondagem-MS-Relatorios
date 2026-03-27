using SME.Sondagem.MS.Relatorios.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class SemestreOuBimestre
{
    public static (string NomeFiltro, string ValorFiltro) ObterFiltroSemestreOuBimestre(int? bimestre, int? semestre, Modalidade modalidade)
    {
        var nomeFiltro = "Bimestre";
        var valorFiltro = "Todos";

        if (bimestre != null && bimestre != 0 && Enum.TryParse(bimestre.ToString(), out Bimestre bimestreEnum))
        {
            valorFiltro = bimestreEnum.ShortName() ?? "Todos";
        }

        return (nomeFiltro, valorFiltro);
    }
}
