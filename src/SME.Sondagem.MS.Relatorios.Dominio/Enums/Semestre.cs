using System.ComponentModel.DataAnnotations;

namespace SME.Sondagem.MS.Relatorios.Dominio.Enums;

public enum Semestre
{
    [Display(Name = "1° semestre", ShortName = "1° semestre")]
    Primeiro = 1,

    [Display(Name = "2° semestre", ShortName = "2° semestre")]
    Segundo = 2,
}
