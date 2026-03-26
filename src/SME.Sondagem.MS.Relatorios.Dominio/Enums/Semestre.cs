using System.ComponentModel.DataAnnotations;

namespace SME.Sondagem.MS.Relatorios.Dominio.Enums;

public enum Semestre
{
    [Display(Name = "1° Semestre", ShortName = "1° Semestre")]
    Primeiro = 1,

    [Display(Name = "2° Semestre", ShortName = "2° Semestre")]
    Segundo = 2,
}
