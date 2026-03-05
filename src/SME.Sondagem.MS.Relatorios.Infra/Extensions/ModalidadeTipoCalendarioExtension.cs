using SME.Sondagem.MS.Relatorios.Infra.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Exceptions;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class ModalidadeTipoCalendarioExtension
{
    public static Modalidade[] ObterModalidades(this ModalidadeTipoCalendario modalidade)
    {
        switch (modalidade)
        {
            case ModalidadeTipoCalendario.FundamentalMedio:
                return new[] { Modalidade.Fundamental, Modalidade.Medio };
            case ModalidadeTipoCalendario.EJA:
                return new[] { Modalidade.EJA };
            case ModalidadeTipoCalendario.Infantil:
                return new[] { Modalidade.Infantil };
            case ModalidadeTipoCalendario.CELP:
                return new[] { Modalidade.CELP };
            default:
                throw new NegocioException("Modalidade de tipo de calendário não identificado para conversão de modalidade de turma");
        }
    }
}