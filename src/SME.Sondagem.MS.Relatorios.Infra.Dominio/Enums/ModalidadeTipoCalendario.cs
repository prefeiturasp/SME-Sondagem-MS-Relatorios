using System.ComponentModel.DataAnnotations;

namespace SME.Sondagem.MS.Relatorios.Infra.Dominio.Enums;

public enum ModalidadeTipoCalendario
{
    [Display(Name = "Fundamental/Médio")]
    FundamentalMedio = 1,

    [Display(Name = "EJA")]
    EJA = 2,

    [Display(Name = "Infantil")]
    Infantil = 3,

    [Display(Name = "CELP")]
    CELP = 4
}
