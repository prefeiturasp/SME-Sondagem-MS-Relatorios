using SME.Sondagem.MS.Relatorios.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class SemestreOuBimestre
{
    public static (string NomeFiltro, string ValorFiltro) ObterFiltroSemestreOuBimestre(string bimestre, string semestre, Modalidade modalidade)
    {
        var nomeFiltro = "";
        var valorFiltro = "Todos";

        if (modalidade == Modalidade.EJA)
        {
            nomeFiltro = "Semestre";

            if (!string.IsNullOrEmpty(semestre) && semestre != "0" && Enum.TryParse(semestre, out Semestre semestreEnum))
            {
                valorFiltro = semestreEnum.ToString();
            }
        }
        else
        {
            nomeFiltro = "Bimestre";

            if (!string.IsNullOrEmpty(bimestre) && bimestre != "0" && Enum.TryParse(bimestre, out Bimestre bimestreEnum))
            {
                valorFiltro = bimestreEnum.ToString();
            }
        }

        return (nomeFiltro, valorFiltro);
    }
}
