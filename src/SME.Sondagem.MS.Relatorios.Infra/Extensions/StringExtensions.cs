using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class StringExtensions
{
    public static int ConverterParaInt(this string valor)
    {
        if (int.TryParse(valor, out int resultado))
            return resultado;

        return 0;
    }
}

