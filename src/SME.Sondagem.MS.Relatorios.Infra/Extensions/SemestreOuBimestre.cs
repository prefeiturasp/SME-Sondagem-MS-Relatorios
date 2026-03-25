using SME.Sondagem.MS.Relatorios.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class SemestreOuBimestre
{
    public static (string NomeFiltro, string ValorFiltro) ObterFiltroSemestreOuBimestre(int? bimestre, int? semestre, Modalidade modalidade)
    {
        var nomeFiltro = "";
        var valorFiltro = "Todos";

        if (modalidade == Modalidade.EJA)
        {
            nomeFiltro = "Semestre";

            if (semestre != null && semestre != 0 && Enum.TryParse(semestre.ToString(), out Semestre semestreEnum))
            {
                valorFiltro = semestreEnum.ShortName() ?? "Todos";
            }
        }
        else
        {
            nomeFiltro = "Bimestre";

            if (bimestre != null && bimestre != 0 && Enum.TryParse(bimestre.ToString(), out Bimestre bimestreEnum))
            {
                valorFiltro = bimestreEnum.ShortName() ?? "Todos";
            }
        }

        return (nomeFiltro, valorFiltro);
    }
}
