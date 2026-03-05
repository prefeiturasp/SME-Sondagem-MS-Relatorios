using SME.Sondagem.MS.Relatorios.Infra.Dominio.Enums;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class ModalidadeExtensao
{
    public static ModalidadeTipoCalendario ObterModalidadeTipoCalendario(this Modalidade modalidade)
    {
        switch (modalidade)
        {
            case Modalidade.Infantil:
                return ModalidadeTipoCalendario.Infantil;
            case Modalidade.EJA:
                return ModalidadeTipoCalendario.EJA;
            case Modalidade.CELP:
                return ModalidadeTipoCalendario.CELP;
            default:
                return ModalidadeTipoCalendario.FundamentalMedio;
        }
    }

    public static bool EhSemestral(this Modalidade modalidade)
    {
        return modalidade == Modalidade.EJA || modalidade == Modalidade.CELP;
    }

    public static bool EhCelp(this Modalidade modalidade)
    {
        return modalidade == Modalidade.CELP;
    }
}
